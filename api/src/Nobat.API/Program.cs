using Nobat.API.Startup;

// ============================================
// نقطه ورود اصلی برنامه
// ============================================

var builder = WebApplication.CreateBuilder(args);

// تنظیمات اولیه: Serilog و سرویس‌ها
StartupConfigurator.ConfigureSerilog(builder);
StartupConfigurator.ConfigureServices(builder.Services, builder.Configuration);

// ساخت برنامه
var app = builder.Build();

// تنظیمات Pipeline: Middleware و Routing
PipelineConfigurator.ConfigurePipeline(app);

// اولیه‌سازی دیتابیس: ایجاد و به‌روزرسانی خودکار
DatabaseInitializer.InitializeDatabase(app);

// اجرای برنامه
app.Run();
