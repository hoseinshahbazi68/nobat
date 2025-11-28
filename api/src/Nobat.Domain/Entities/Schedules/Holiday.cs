using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Schedules;

/// <summary>
/// موجودیت روز تعطیل
/// این موجودیت روزهای تعطیل رسمی و غیررسمی را نگهداری می‌کند
/// برای جلوگیری از نوبت‌دهی در روزهای تعطیل استفاده می‌شود
/// </summary>
public class Holiday : BaseEntity
{
    /// <summary>
    /// تاریخ تعطیل
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// نام تعطیل
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}
