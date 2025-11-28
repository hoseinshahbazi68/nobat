using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Nobat.Domain.Common;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Services;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Entities.Insurances;
using Nobat.Domain.Entities.Common;
using Nobat.Domain.Entities.Chat;
using Nobat.Domain.Entities.Appointments;
using Nobat.Domain.Entities.Clinics;
using Nobat.Domain.Entities.Locations;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Interfaces;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace Nobat.Infrastructure.Data;

    /// <summary>
    /// کانتکست پایگاه داده برنامه
    /// </summary>
    public class ApplicationDbContext : DbContext, IUnitOfWork
    {
        private readonly IServiceProvider? _serviceProvider;
        private bool _isSavingLogs = false;

    /// <summary>
    /// سازنده کانتکست پایگاه داده
    /// </summary>
    /// <param name="options">گزینه‌های کانتکست</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// سازنده کانتکست پایگاه داده با دسترسی به ServiceProvider
    /// </summary>
    /// <param name="options">گزینه‌های کانتکست</param>
    /// <param name="serviceProvider">ارائه‌دهنده سرویس</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IServiceProvider? serviceProvider = null) : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// مجموعه کاربران
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// مجموعه نقش‌ها
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>
    /// مجموعه ارتباطات کاربر و نقش
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; }

    /// <summary>
    /// مجموعه شیفت‌ها
    /// </summary>
    public DbSet<Shift> Shifts { get; set; }

    /// <summary>
    /// مجموعه پزشکان
    /// </summary>
    public DbSet<Doctor> Doctors { get; set; }

    /// <summary>
    /// مجموعه اطلاعات ویزیت پزشک
    /// </summary>
    public DbSet<DoctorVisitInfo> DoctorVisitInfos { get; set; }

    /// <summary>
    /// مجموعه تخصص‌ها
    /// </summary>
    public DbSet<Specialty> Specialties { get; set; }

    /// <summary>
    /// مجموعه روزهای تعطیل
    /// </summary>
    public DbSet<Holiday> Holidays { get; set; }

    /// <summary>
    /// مجموعه بیمه‌ها
    /// </summary>
    public DbSet<Insurance> Insurances { get; set; }

    /// <summary>
    /// مجموعه خدمات
    /// </summary>
    public DbSet<Service> Services { get; set; }

    /// <summary>
    /// مجموعه تعرفه‌های خدمات
    /// </summary>
    public DbSet<ServiceTariff> ServiceTariffs { get; set; }

    /// <summary>
    /// مجموعه برنامه‌های پزشکان
    /// </summary>
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    /// <summary>
    /// مجموعه ارتباطات پزشک و تخصص
    /// </summary>
    public DbSet<DoctorSpecialty> DoctorSpecialties { get; set; }

    /// <summary>
    /// مجموعه نوبت‌ها
    /// </summary>
    public DbSet<Appointment> Appointments { get; set; }

    /// <summary>
    /// مجموعه لاگ فعالیت‌های کاربران
    /// </summary>
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }

    /// <summary>
    /// مجموعه لاگ تغییرات دیتابیس
    /// </summary>
    public DbSet<DatabaseChangeLog> DatabaseChangeLogs { get; set; }

    /// <summary>
    /// مجموعه لاگ کوئری‌های دیتابیس
    /// </summary>
    public DbSet<QueryLog> QueryLogs { get; set; }

    /// <summary>
    /// مجموعه پیام‌های چت
    /// </summary>
    public DbSet<ChatMessage> ChatMessages { get; set; }

    /// <summary>
    /// مجموعه جلسه‌های چت
    /// </summary>
    public DbSet<ChatSession> ChatSessions { get; set; }

    /// <summary>
    /// مجموعه کلینیک‌ها
    /// </summary>
    public DbSet<Clinic> Clinics { get; set; }

    /// <summary>
    /// مجموعه ارتباطات پزشک و کلینیک
    /// </summary>
    public DbSet<DoctorClinic> DoctorClinics { get; set; }

    /// <summary>
    /// مجموعه ارتباطات کلینیک و کاربر (مدیران کلینیک)
    /// </summary>
    public DbSet<ClinicUser> ClinicUsers { get; set; }

    /// <summary>
    /// مجموعه استان‌ها
    /// </summary>
    public DbSet<Province> Provinces { get; set; }

    /// <summary>
    /// مجموعه شهرها
    /// </summary>
    public DbSet<City> Cities { get; set; }


    /// <summary>
    /// مجموعه علائم و مشکلات پزشکی
    /// </summary>
    public DbSet<MedicalCondition> MedicalConditions { get; set; }

    /// <summary>
    /// مجموعه ارتباطات تخصص و علائم پزشکی
    /// </summary>
    public DbSet<SpecialtyMedicalCondition> SpecialtyMedicalConditions { get; set; }

    /// <summary>
    /// مجموعه ارتباطات پزشک و علائم پزشکی
    /// </summary>
    public DbSet<DoctorMedicalCondition> DoctorMedicalConditions { get; set; }

    /// <summary>
    /// پیکربندی مدل پایگاه داده
    /// </summary>
    /// <param name="modelBuilder">سازنده مدل</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Seed data
        SeedData.SeedRoles(modelBuilder);
        SeedData.SeedUsers(modelBuilder);
        SeedData.SeedShifts(modelBuilder);
        SeedData.SeedClinics(modelBuilder);
        SeedData.SeedClinicUsers(modelBuilder);
        SeedData.SeedDoctors(modelBuilder);
        SeedData.SeedDoctorClinics(modelBuilder);
        SeedData.SeedSpecialties(modelBuilder);
        SeedData.SeedDoctorSpecialties(modelBuilder);
        SeedData.SeedMedicalConditions(modelBuilder);
        SeedData.SeedSpecialtyMedicalConditions(modelBuilder);
        SeedData.SeedInsurances(modelBuilder);
        SeedData.SeedServices(modelBuilder);
        SeedData.SeedProvinces(modelBuilder);
        SeedData.SeedCities(modelBuilder);

        // Global query filter for soft delete - removed for now due to complexity
        // Can be added per entity if needed
    }

    /// <summary>
    /// ذخیره تغییرات در پایگاه داده
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>تعداد ردیف‌های تأثیر یافته</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // اگر در حال ذخیره لاگ‌ها هستیم، از لاگ کردن مجدد جلوگیری می‌کنیم
        if (_isSavingLogs)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        // دریافت شناسه کاربر فعلی
        int? currentUserId = GetCurrentUserId();
        string? ipAddress = GetCurrentIpAddress();

        // به‌روزرسانی CreatedAt و UpdatedAt
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // لاگ کردن تغییرات دیتابیس (قبل از ذخیره)
        var changeLogs = new List<DatabaseChangeLog>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            // از لاگ کردن خود جداول لاگ جلوگیری می‌کنیم
            if (entry.Entity is UserActivityLog || entry.Entity is DatabaseChangeLog || entry.Entity is QueryLog)
                continue;

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                var changeLog = CreateChangeLog(entry, currentUserId, ipAddress);
                if (changeLog != null)
                {
                    changeLogs.Add(changeLog);
                }
            }
        }

        // ذخیره تغییرات اصلی
        var result = await base.SaveChangesAsync(cancellationToken);

        // ذخیره لاگ‌های تغییرات (بعد از ذخیره موفق)
        if (changeLogs.Any())
        {
            _isSavingLogs = true;
            try
            {
                foreach (var log in changeLogs)
                {
                    // تنظیم State به Added برای جلوگیری از لاگ کردن خود لاگ
                    Entry(log).State = EntityState.Added;
                }
                await base.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _isSavingLogs = false;
            }
        }

        return result;
    }

    /// <summary>
    /// ایجاد لاگ تغییرات برای یک موجودیت
    /// </summary>
    private DatabaseChangeLog? CreateChangeLog(EntityEntry<BaseEntity> entry, int? userId, string? ipAddress)
    {
        var entityType = entry.Entity.GetType();
        var tableName = entityType.Name;

        string changeType = entry.State switch
        {
            EntityState.Added => "Added",
            EntityState.Modified => "Modified",
            EntityState.Deleted => "Deleted",
            _ => "Unknown"
        };

        var recordId = entry.Entity.Id.ToString();
        var changedColumns = new List<string>();
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        if (entry.State == EntityState.Modified)
        {
            foreach (var property in entry.Properties)
            {
                if (property.IsModified && property.Metadata.Name != "UpdatedAt")
                {
                    changedColumns.Add(property.Metadata.Name);
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                    newValues[property.Metadata.Name] = property.CurrentValue;
                }
            }
        }
        else if (entry.State == EntityState.Added)
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name != "Id" && property.Metadata.Name != "CreatedAt")
                {
                    newValues[property.Metadata.Name] = property.CurrentValue;
                }
            }
        }
        else if (entry.State == EntityState.Deleted)
        {
            foreach (var property in entry.Properties)
            {
                oldValues[property.Metadata.Name] = property.OriginalValue;
            }
        }

        return new DatabaseChangeLog
        {
            UserId = userId,
            TableName = tableName,
            RecordId = recordId,
            ChangeType = changeType,
            ChangedColumns = changedColumns.Any() ? JsonSerializer.Serialize(changedColumns) : null,
            OldValues = oldValues.Any() ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues.Any() ? JsonSerializer.Serialize(newValues) : null,
            ChangeTime = DateTime.UtcNow,
            IpAddress = ipAddress
        };
    }

    /// <summary>
    /// دریافت شناسه کاربر فعلی از HttpContext
    /// </summary>
    private int? GetCurrentUserId()
    {
        var httpContextAccessor = _serviceProvider?.GetService<IHttpContextAccessor>();
        if (httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
        }
        return null;
    }

    /// <summary>
    /// دریافت آدرس IP کاربر فعلی
    /// </summary>
    private string? GetCurrentIpAddress()
    {
        var httpContextAccessor = _serviceProvider?.GetService<IHttpContextAccessor>();
        if (httpContextAccessor?.HttpContext != null)
        {
            var httpContext = httpContextAccessor.HttpContext;

            // بررسی X-Forwarded-For برای پروکسی
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim();
            }

            // بررسی X-Real-IP
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // استفاده از RemoteIpAddress
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
        return null;
    }

    /// <summary>
    /// شروع تراکنش جدید
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// تأیید و ذخیره تراکنش
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// بازگشت تراکنش
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.RollbackTransactionAsync(cancellationToken);
    }
}
