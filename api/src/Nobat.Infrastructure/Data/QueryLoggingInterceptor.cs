using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nobat.Domain.Entities.Common;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;

namespace Nobat.Infrastructure.Data;

/// <summary>
/// Interceptor برای لاگ‌گیری کوئری‌های دیتابیس
/// </summary>
public class QueryLoggingInterceptor : DbCommandInterceptor
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<QueryLoggingInterceptor> _logger;
    private readonly IServiceProvider _serviceProvider;

    // استفاده از ConcurrentDictionary برای thread-safety
    private static readonly ConcurrentDictionary<string, Stopwatch> _stopwatches = new();
    private static readonly ConcurrentDictionary<string, QueryLogInfo> _queryInfos = new();

    // حد آستانه زمان اجرا برای کوئری‌های سنگین (به میلی‌ثانیه) - از تنظیمات خوانده می‌شود
    private readonly long _heavyQueryThresholdMs;
    private readonly bool _isEnabled;

    public QueryLoggingInterceptor(
        IConfiguration configuration,
        ILogger<QueryLoggingInterceptor> logger,
        IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _isEnabled = _configuration.GetValue<bool>("QueryLogging:Enabled", true);
        _heavyQueryThresholdMs = _configuration.GetValue<long>("QueryLogging:HeavyQueryThresholdMs", 1000); // پیش‌فرض 1 ثانیه
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        if (!_isEnabled)
            return base.ReaderExecuting(command, eventData, result);

        var commandId = GetCommandId(command);
        var stopwatch = Stopwatch.StartNew();
        _stopwatches[commandId] = stopwatch;

        // استخراج اطلاعات کوئری
        var queryInfo = new QueryLogInfo
        {
            CommandId = commandId,
            CommandText = command.CommandText,
            CommandType = command.CommandType.ToString(),
            Parameters = ExtractParameters(command),
            StartTime = DateTime.UtcNow
        };

        _queryInfos[commandId] = queryInfo;

        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);

        var commandId = GetCommandId(command);
        var stopwatch = Stopwatch.StartNew();
        _stopwatches[commandId] = stopwatch;

        var queryInfo = new QueryLogInfo
        {
            CommandId = commandId,
            CommandText = command.CommandText,
            CommandType = command.CommandType.ToString(),
            Parameters = ExtractParameters(command),
            StartTime = DateTime.UtcNow
        };

        _queryInfos[commandId] = queryInfo;

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        if (_isEnabled)
        {
            ProcessCommandExecution(command, eventData);
        }

        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        if (_isEnabled)
        {
            ProcessCommandExecution(command, eventData);
        }

        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void ProcessCommandExecution(DbCommand command, CommandExecutedEventData eventData)
    {
        var commandId = GetCommandId(command);

        if (_stopwatches.TryRemove(commandId, out var stopwatch))
        {
            stopwatch.Stop();
            var executionTimeMs = stopwatch.ElapsedMilliseconds;

            if (_queryInfos.TryRemove(commandId, out var queryInfo))
            {
                SaveQueryLog(queryInfo, executionTimeMs, null);
            }
        }
    }

    private string GetCommandId(DbCommand command)
    {
        // استفاده از HashCode برای شناسایی یکتای command
        return $"{command.GetHashCode()}_{command.CommandText?.GetHashCode() ?? 0}";
    }

    private string? ExtractParameters(DbCommand command)
    {
        if (command.Parameters.Count == 0)
            return null;

        var parameters = new Dictionary<string, object?>();
        foreach (DbParameter param in command.Parameters)
        {
            parameters[param.ParameterName] = param.Value;
        }

        return JsonSerializer.Serialize(parameters);
    }

    private void SaveQueryLog(QueryLogInfo queryInfo, long executionTimeMs, Exception? exception)
    {
        try
        {
            // فقط کوئری‌های سنگین یا کوئری‌هایی که خطا داشته‌اند را لاگ می‌کنیم
            // یا اگر تنظیم شده که همه کوئری‌ها را لاگ کنیم
            var logAllQueries = _configuration.GetValue<bool>("QueryLogging:LogAllQueries", false);
            var isHeavy = executionTimeMs >= _heavyQueryThresholdMs;

            if (!logAllQueries && !isHeavy && exception == null)
                return;

            // استفاده از Background Service یا Task برای ذخیره لاگ به صورت غیرهمزمان
            // تا روی عملکرد تأثیر نگذارد
            Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // جلوگیری از لاگ کردن خود جدول QueryLog
                    if (queryInfo.CommandText.Contains("QueryLogs", StringComparison.OrdinalIgnoreCase))
                        return;

                    var httpContextAccessor = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
                    var userId = GetUserId(httpContextAccessor);
                    var ipAddress = GetIpAddress(httpContextAccessor);
                    var controllerAction = GetControllerAction(httpContextAccessor);

                    var tablesUsed = ExtractTableNames(queryInfo.CommandText);

                    var queryLog = new QueryLog
                    {
                        UserId = userId,
                        QueryText = queryInfo.CommandText.Length > 10000 ? queryInfo.CommandText.Substring(0, 10000) : queryInfo.CommandText,
                        Parameters = queryInfo.Parameters,
                        ExecutionTimeMs = executionTimeMs,
                        ExecutionTime = queryInfo.StartTime,
                        CommandType = queryInfo.CommandType,
                        TablesUsed = tablesUsed,
                        IpAddress = ipAddress,
                        ControllerAction = controllerAction,
                        IsHeavy = isHeavy,
                        ErrorMessage = exception?.Message
                    };

                    context.QueryLogs.Add(queryLog);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطا در ذخیره لاگ کوئری");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در لاگ‌گیری کوئری");
        }
    }

    private int? GetUserId(Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor)
    {
        if (httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }

        return null;
    }

    private string? GetIpAddress(Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor)
    {
        if (httpContextAccessor?.HttpContext == null)
            return null;

        var httpContext = httpContextAccessor.HttpContext;

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetControllerAction(Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor)
    {
        if (httpContextAccessor?.HttpContext == null)
            return null;

        var routeData = httpContextAccessor.HttpContext.Request.RouteValues;
        if (routeData.TryGetValue("controller", out var controller) && routeData.TryGetValue("action", out var action))
        {
            return $"{controller}/{action}";
        }

        return null;
    }

    private string? ExtractTableNames(string queryText)
    {
        // استخراج نام جداول از کوئری SQL (ساده)
        // این یک روش ساده است و ممکن است برای همه کوئری‌ها کار نکند
        var tables = new List<string>();

        // استخراج جداول از FROM و JOIN
        var regex = new System.Text.RegularExpressions.Regex(@"\b(?:FROM|JOIN)\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var matches = regex.Matches(queryText);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var tableName = match.Groups[1].Value;
                if (!tables.Contains(tableName, StringComparer.OrdinalIgnoreCase))
                {
                    tables.Add(tableName);
                }
            }
        }

        return tables.Any() ? string.Join(", ", tables) : null;
    }

    private class QueryLogInfo
    {
        public string CommandId { get; set; } = string.Empty;
        public string CommandText { get; set; } = string.Empty;
        public string? CommandType { get; set; }
        public string? Parameters { get; set; }
        public DateTime StartTime { get; set; }
    }
}
