using Nobat.Domain.Common;
using Nobat.Domain.Entities.Users;

namespace Nobat.Domain.Entities.Common;

/// <summary>
/// موجودیت لاگ تغییرات دیتابیس
/// </summary>
public class DatabaseChangeLog : BaseEntity
{
    /// <summary>
    /// شناسه کاربر که تغییر را ایجاد کرده (اختیاری)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// نام جدول
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// شناسه رکورد تغییر یافته
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// نوع تغییر (Added, Modified, Deleted)
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// نام ستون‌های تغییر یافته (JSON array)
    /// </summary>
    public string? ChangedColumns { get; set; }

    /// <summary>
    /// مقادیر قدیمی (JSON)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// مقادیر جدید (JSON)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// زمان تغییر
    /// </summary>
    public DateTime ChangeTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// آدرس IP کاربر
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// اطلاعات اضافی (JSON)
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// ارتباط با کاربر (اختیاری)
    /// </summary>
    public virtual User? User { get; set; }
}
