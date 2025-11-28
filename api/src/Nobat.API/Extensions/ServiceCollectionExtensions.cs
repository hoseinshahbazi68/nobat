using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nobat.Application.Doctors;
using Nobat.Application.Insurances;
using Nobat.Application.Clinics;
using Nobat.Application.Jwt;
using Nobat.Application.Mappings;
using Nobat.Application.QueryLogs;
using Nobat.Application.DatabaseChangeLogs;
using Nobat.Application.Repositories;
using Nobat.Application.Schedules;
using Nobat.Application.Services;
using Nobat.Application.Users;
using Nobat.Application.Appointments;
using Nobat.Application.Users.Dto;
using Nobat.Application.Chat;
using Nobat.Application.Locations;
using Nobat.Application.Common;
using Nobat.Domain.Interfaces;
using Nobat.Infrastructure.Data;
using Nobat.Infrastructure.Repositories;
using Sieve.Services;
using System.Text;
using Nobat.Infrastructure.SupportStatus;

namespace Nobat.API.Extensions;

/// <summary>
/// کلاس توسعه‌دهنده برای ثبت سرویس‌ها
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// افزودن پایگاه داده
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <param name="configuration">تنظیمات</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // افزودن IHttpContextAccessor برای دسترسی به HttpContext در DbContext
        services.AddHttpContextAccessor();

        // ثبت Query Logging Interceptor
        services.AddSingleton<QueryLoggingInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                // افزایش Command Timeout به 90 ثانیه برای جلوگیری از timeout در query‌های سنگین
                sqlServerOptions.CommandTimeout(90);
                // فعال کردن Retry on Failure برای اتصالات دیتابیس
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            // افزودن Query Logging Interceptor
            var interceptor = serviceProvider.GetRequiredService<QueryLoggingInterceptor>();
            options.AddInterceptors(interceptor);
        }, ServiceLifetime.Scoped);

        // Override ApplicationDbContext registration to inject ServiceProvider
        services.AddScoped<ApplicationDbContext>(provider =>
        {
            var options = provider.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            return new ApplicationDbContext(options, provider);
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    /// <summary>
    /// افزودن ریپازیتوری‌ها
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // ثبت Generic Repository برای همه موجودیت‌ها
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    /// <summary>
    /// افزودن سرویس‌های اپلیکیشن
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IDoctorVisitInfoService, DoctorVisitInfoService>();
        services.AddScoped<IClinicService, ClinicService>();
        services.AddScoped<ISpecialtyService, SpecialtyService>();
        services.AddScoped<IMedicalConditionService, MedicalConditionService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IInsuranceService, InsuranceService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IServiceTariffService, ServiceTariffService>();
        services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IQueryLogService, QueryLogService>();
        services.AddScoped<IUserActivityLogService, UserActivityLogService>();
        services.AddScoped<IDatabaseChangeLogService, DatabaseChangeLogService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ISupportStatusService, SupportStatusService>();
        services.AddScoped<IChatbotService, ChatbotService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IProvinceService, ProvinceService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IFileUploadService, FileUploadService>();

        return services;
    }

    /// <summary>
    /// افزودن AutoMapper
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }

    /// <summary>
    /// افزودن احراز هویت JWT
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <param name="configuration">تنظیمات</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:SecretKey"] ?? throw new ArgumentNullException("Jwt:SecretKey");
        var issuer = configuration["Jwt:Issuer"] ?? "NobatAPI";
        var audience = configuration["Jwt:Audience"] ?? "NobatAPI";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("User", policy => policy.RequireRole("User", "Admin"));
        });

        return services;
    }

    /// <summary>
    /// افزودن کش Redis
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <param name="configuration">تنظیمات</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var isEnabled = configuration.GetValue<bool>("Cache:Redis:Enabled", true);

        if (isEnabled)
        {
            var redisConnection = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                });
            }
            else
            {
                // Fallback to in-memory cache if Redis connection string is not provided
                services.AddDistributedMemoryCache();
            }
        }
        else
        {
            // Use in-memory cache when Redis is disabled
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    /// <summary>
    /// افزودن محدودیت نرخ درخواست بر اساس IP
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <param name="configuration">تنظیمات</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddIpRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }

    /// <summary>
    /// افزودن نسخه‌بندی API
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("version"),
                new HeaderApiVersionReader("X-Version"),
                new UrlSegmentApiVersionReader()
            );
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// افزودن Swagger
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Nobat API",
                Description = "Nobat API with versioning support",
                Contact = new OpenApiContact
                {
                    Name = "Nobat Team"
                }
            });

            // Add JWT authentication to Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments from API project
            var apiAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var apiXmlFile = $"{apiAssembly.GetName().Name}.xml";
            var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
            if (File.Exists(apiXmlPath))
            {
                options.IncludeXmlComments(apiXmlPath);
            }

            // Include XML comments from Application project (for DTOs)
            var applicationAssembly = typeof(UserDto).Assembly;
            var applicationXmlFile = $"{applicationAssembly.GetName().Name}.xml";
            // Try to find XML file in the same directory as the assembly
            var assemblyLocation = applicationAssembly.Location;
            if (!string.IsNullOrEmpty(assemblyLocation))
            {
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                if (!string.IsNullOrEmpty(assemblyDirectory))
                {
                    var applicationXmlPath = Path.Combine(assemblyDirectory, applicationXmlFile);
                    if (File.Exists(applicationXmlPath))
                    {
                        options.IncludeXmlComments(applicationXmlPath);
                    }
                }
            }
            // Fallback to AppContext.BaseDirectory if assembly location is not available
            var fallbackXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
            if (File.Exists(fallbackXmlPath))
            {
                options.IncludeXmlComments(fallbackXmlPath);
            }
        });

        return services;
    }

    /// <summary>
    /// افزودن Sieve برای فیلتر و مرتب‌سازی
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddSieve(this IServiceCollection services)
    {
        services.AddScoped<ISieveProcessor, SieveProcessor>();
        return services;
    }

    /// <summary>
    /// افزودن MVC حداقلی
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <returns>مجموعه سرویس‌ها</returns>
    public static IServiceCollection AddMinimalMvc(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        return services;
    }
}
