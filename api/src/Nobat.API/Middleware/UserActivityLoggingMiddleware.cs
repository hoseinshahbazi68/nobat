using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nobat.Domain.Entities.Users;
using Nobat.Infrastructure.Data;
using System.Security.Claims;
using System.Text.Json;

namespace Nobat.API.Middleware;

/// <summary>
/// میدلور لاگ کردن فعالیت‌های کاربران
/// </summary>
public class UserActivityLoggingMiddleware
{
    /// <summary>
    /// درخواست بعدی در pipeline
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// لاگر
    /// </summary>
    private readonly ILogger<UserActivityLoggingMiddleware> _logger;

    /// <summary>
    /// سازنده میدلور لاگ فعالیت‌های کاربران
    /// </summary>
    /// <param name="next">درخواست بعدی</param>
    /// <param name="logger">لاگر</param>
    public UserActivityLoggingMiddleware(RequestDelegate next, ILogger<UserActivityLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// اجرای میدلور
    /// </summary>
    /// <param name="context">کانتکست HTTP</param>
    /// <param name="serviceScopeFactory">فکتوری برای ایجاد scope</param>
    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory serviceScopeFactory)
    {
        // دریافت اطلاعات درخواست
        var requestPath = context.Request.Path.Value;
        var httpMethod = context.Request.Method;
        var ipAddress = GetClientIpAddress(context);
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        // دریافت اطلاعات کاربر
        int? userId = null;
        string? username = null;
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                userId = parsedUserId;
            }
            username = context.User.FindFirst(ClaimTypes.Name)?.Value;
        }

        // تعیین نوع فعالیت بر اساس مسیر و متد
        var action = DetermineAction(requestPath, httpMethod);

        // استخراج نام کنترلر از مسیر
        var controller = ExtractControllerName(requestPath);

        // خواندن body درخواست (فقط برای POST, PUT, PATCH)
        string? requestBody = null;
        if (httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "PATCH")
        {
            context.Request.EnableBuffering();
            var bodyStream = new StreamReader(context.Request.Body);
            requestBody = await bodyStream.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var statusCode = 200;
        string? errorMessage = null;
        string? additionalData = null;

        // اجرای درخواست و ثبت لاگ
        try
        {
            await _next(context);
            statusCode = context.Response.StatusCode;
        }
        catch (Exception ex)
        {
            statusCode = 500;
            errorMessage = ex.Message;
            _logger.LogError(ex, "Error in UserActivityLoggingMiddleware");
            throw;
        }
        finally
        {
            // ثبت لاگ فعالیت (به صورت غیرهمزمان و بدون انتظار)
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    await LogUserActivityAsync(
                        dbContext,
                        userId,
                        username,
                        action,
                        controller,
                        httpMethod,
                        requestPath,
                        ipAddress,
                        userAgent,
                        statusCode,
                        errorMessage,
                        requestBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error logging user activity");
                }
            });
        }
    }

    /// <summary>
    /// ثبت لاگ فعالیت کاربر
    /// </summary>
    private async Task LogUserActivityAsync(
        ApplicationDbContext dbContext,
        int? userId,
        string? username,
        string action,
        string? controller,
        string httpMethod,
        string? requestPath,
        string? ipAddress,
        string? userAgent,
        int statusCode,
        string? errorMessage,
        string? requestBody)
    {
        try
        {
            var additionalDataDict = new Dictionary<string, object?>();
            if (!string.IsNullOrEmpty(requestBody))
            {
                additionalDataDict["RequestBody"] = requestBody;
            }

            var activityLog = new UserActivityLog
            {
                UserId = userId,
                Username = username,
                Action = action,
                Controller = controller,
                HttpMethod = httpMethod,
                RequestPath = requestPath,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                StatusCode = statusCode,
                ErrorMessage = errorMessage,
                AdditionalData = additionalDataDict.Any() ? JsonSerializer.Serialize(additionalDataDict) : null,
                ActivityTime = DateTime.UtcNow
            };

            dbContext.UserActivityLogs.Add(activityLog);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user activity log");
        }
    }

    /// <summary>
    /// تعیین نوع فعالیت بر اساس مسیر و متد HTTP
    /// </summary>
    private string DetermineAction(string? requestPath, string httpMethod)
    {
        if (string.IsNullOrEmpty(requestPath))
            return "Unknown";

        var pathLower = requestPath.ToLower();

        // لاگین و لاگ‌آوت
        if (pathLower.Contains("/auth/login"))
            return "Login";
        if (pathLower.Contains("/auth/logout"))
            return "Logout";
        if (pathLower.Contains("/auth/register"))
            return "Register";

        // عملیات CRUD بر اساس متد HTTP
        return httpMethod switch
        {
            "GET" => "View",
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// استخراج نام کنترلر از مسیر
    /// </summary>
    private string? ExtractControllerName(string? requestPath)
    {
        if (string.IsNullOrEmpty(requestPath))
            return null;

        var segments = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 2)
        {
            // معمولاً فرمت: /api/v1/ControllerName/...
            var controllerIndex = Array.IndexOf(segments, "v1") + 1;
            if (controllerIndex > 0 && controllerIndex < segments.Length)
            {
                return segments[controllerIndex];
            }
        }

        return null;
    }

    /// <summary>
    /// دریافت آدرس IP کلاینت
    /// </summary>
    private string? GetClientIpAddress(HttpContext context)
    {
        // بررسی X-Forwarded-For برای پروکسی
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        // بررسی X-Real-IP
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // استفاده از RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
