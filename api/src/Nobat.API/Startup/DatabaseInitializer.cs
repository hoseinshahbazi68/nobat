using Microsoft.EntityFrameworkCore;
using Nobat.Infrastructure.Data;
using Serilog;

namespace Nobat.API.Startup;

/// <summary>
/// کلاس مدیریت اولیه دیتابیس
/// این کلاس مسئول ایجاد و به‌روزرسانی خودکار دیتابیس است
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// ایجاد و به‌روزرسانی خودکار دیتابیس
    /// این متد دیتابیس را ایجاد می‌کند اگر وجود نداشته باشد
    /// و Migration های موجود را اعمال می‌کند
    /// </summary>
    /// <param name="app">سازنده برنامه</param>
    public static void InitializeDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // استفاده از EnsureCreated برای ایجاد خودکار دیتابیس بدون نیاز به Migration
            // این متد دیتابیس را ایجاد می‌کند اگر وجود نداشته باشد
            // و جداول را بر اساس Entity های موجود می‌سازد
            var created = context.Database.EnsureCreated();

            if (created)
            {
                Log.Information("Database and tables created successfully");
            }
            else
            {
                Log.Information("Database already exists");

                // اگر دیتابیس وجود دارد، سعی می‌کنیم Migration ها را اعمال کنیم (اگر وجود داشته باشند)
                ApplyPendingMigrations(context);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while initializing the database");
            throw; // در صورت خطا، برنامه متوقف می‌شود
        }
    }

    /// <summary>
    /// اعمال Migration های موجود
    /// </summary>
    /// <param name="context">کانتکست دیتابیس</param>
    private static void ApplyPendingMigrations(ApplicationDbContext context)
    {
        try
        {
            var pendingMigrations = context.Database.GetPendingMigrations().ToList();
            if (pendingMigrations.Any())
            {
                context.Database.Migrate();
                Log.Information("Applied pending migrations: {Migrations}", string.Join(", ", pendingMigrations));
            }
        }
        catch (Exception migrateEx)
        {
            // اگر Migration با خطا مواجه شد، فقط لاگ می‌کنیم
            // چون EnsureCreated قبلاً اجرا شده و دیتابیس وجود دارد
            Log.Warning(migrateEx, "Could not apply migrations, but database exists");
        }
    }
}
