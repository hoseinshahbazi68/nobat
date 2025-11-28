using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Services;

/// <summary>
/// موجودیت خدمت
/// این موجودیت خدمات قابل ارائه در سیستم را نگهداری می‌کند
/// شامل نام خدمت و توضیحات
/// </summary>
public class Service : BaseEntity
{
    /// <summary>
    /// نام خدمت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// مجموعه تعرفه‌های خدمات
    /// </summary>
    public virtual ICollection<ServiceTariff> ServiceTariffs { get; set; } = new List<ServiceTariff>();
}
