namespace Nobat.Domain.Common;

/// <summary>
/// کلاس پایه برای تمام موجودیت‌های دامنه
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// شناسه یکتای موجودیت
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// تاریخ و زمان ایجاد موجودیت
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاریخ و زمان آخرین بروزرسانی موجودیت
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
