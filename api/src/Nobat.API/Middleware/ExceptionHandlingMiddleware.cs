using System.Net;
using System.Text.Json;

namespace Nobat.API.Middleware;

/// <summary>
/// میدلور مدیریت خطاها
/// </summary>
public class ExceptionHandlingMiddleware
{
    /// <summary>
    /// درخواست بعدی در پipeline
    /// </summary>
    private readonly RequestDelegate _next;
    
    /// <summary>
    /// لاگر
    /// </summary>
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// سازنده میدلور مدیریت خطاها
    /// </summary>
    /// <param name="next">درخواست بعدی</param>
    /// <param name="logger">لاگر</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// اجرای میدلور
    /// </summary>
    /// <param name="context">کانتکست HTTP</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// مدیریت خطا و ایجاد پاسخ مناسب
    /// </summary>
    /// <param name="context">کانتکست HTTP</param>
    /// <param name="exception">خطای رخ داده</param>
    /// <returns>تسک</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;
            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;
            default:
                result = JsonSerializer.Serialize(new { error = "An error occurred while processing your request." });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}

