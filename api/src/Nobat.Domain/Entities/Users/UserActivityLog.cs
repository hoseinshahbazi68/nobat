using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Users;

/// <summary>
/// موجودیت لاگ فعالیت‌های کاربران
/// </summary>
public class UserActivityLog : BaseEntity
{
    /// <summary>
    /// شناسه کاربر (اختیاری - در صورت لاگین بودن)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// نام کاربری (در صورت عدم لاگین بودن)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// نوع فعالیت (مثلاً: Login, Logout, Create, Update, Delete, View)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// کنترلر مربوطه
    /// </summary>
    public string? Controller { get; set; }

    /// <summary>
    /// متد HTTP (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// مسیر درخواست
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// آدرس IP کاربر
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent مرورگر
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// اطلاعات اضافی (JSON)
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// وضعیت پاسخ (200, 400, 500, etc.)
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// پیام خطا (در صورت وجود)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// زمان انجام فعالیت
    /// </summary>
    public DateTime ActivityTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ارتباط با کاربر (اختیاری)
    /// </summary>
    public virtual User? User { get; set; }
}
