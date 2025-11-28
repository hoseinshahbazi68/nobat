using AspNetCoreRateLimit;
using Nobat.API.Extensions;
using Nobat.Infrastructure.Job;
using Serilog;

namespace Nobat.API.Startup;

/// <summary>
/// کلاس تنظیمات اولیه برنامه
/// این کلاس مسئول تنظیمات اولیه مانند Serilog و ثبت سرویس‌ها است
/// </summary>
public static class StartupConfigurator
{
    /// <summary>
    /// تنظیم Serilog برای لاگ‌گیری
    /// </summary>
    /// <param name="builder">سازنده برنامه</param>
    public static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/nobat-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    /// <summary>
    /// ثبت تمام سرویس‌های مورد نیاز برنامه
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <param name="configuration">تنظیمات</param>
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // ثبت سرویس‌های پایه
        services.AddDatabase(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddAutoMapper();
        services.AddSieve();
        services.AddMinimalMvc();
        services.AddCustomApiVersioning();
        services.AddSwagger();
        services.AddJwtAuthentication(configuration);
        services.AddRedisCache(configuration);
        services.AddIpRateLimiting(configuration);

        // ثبت سرویس‌های پس‌زمینه
        services.AddHostedService<ChatCleanupService>();
        services.AddHostedService<AppointmentGenerationService>();

        // تنظیم CORS
        ConfigureCors(services);
    }

    /// <summary>
    /// تنظیم CORS برای دسترسی از همه منابع
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    private static void ConfigureCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }
}
