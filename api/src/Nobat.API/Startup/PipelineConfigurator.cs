using AspNetCoreRateLimit;
using Nobat.API.Middleware;

namespace Nobat.API.Startup;

/// <summary>
/// کلاس تنظیمات Pipeline درخواست‌های HTTP
/// این کلاس مسئول تنظیمات Middleware و Pipeline است
/// </summary>
public static class PipelineConfigurator
{
    /// <summary>
    /// تنظیم Pipeline برای درخواست‌های HTTP
    /// </summary>
    /// <param name="app">سازنده برنامه</param>
    public static void ConfigurePipeline(WebApplication app)
    {
        // فعال‌سازی فایل‌های استاتیک (باید قبل از Swagger UI باشد)
        app.UseStaticFiles();

        // تنظیم Swagger در محیط Development
        if (app.Environment.IsDevelopment())
        {
            ConfigureSwagger(app);
        }

        // تنظیمات امنیتی و Middleware
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<UserActivityLoggingMiddleware>();
        app.UseIpRateLimiting();

        // تنظیمات احراز هویت
        app.UseAuthentication();
        app.UseAuthorization();

        // تنظیم Controller ها
        app.MapControllers();
    }

    /// <summary>
    /// تنظیم Swagger UI
    /// </summary>
    /// <param name="app">سازنده برنامه</param>
    private static void ConfigureSwagger(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nobat API v1");
            options.RoutePrefix = string.Empty;
            options.InjectJavascript("/js/swagger-login.js");
        });
    }
}
