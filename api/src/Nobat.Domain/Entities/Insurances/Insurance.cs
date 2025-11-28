using Nobat.Domain.Common;
using Nobat.Domain.Entities.Services;

namespace Nobat.Domain.Entities.Insurances;

/// <summary>
/// موجودیت بیمه
/// این موجودیت اطلاعات بیمه‌های طرف قرارداد را نگهداری می‌کند
/// شامل نام و کد بیمه
/// </summary>
public class Insurance : BaseEntity
{
    /// <summary>
    /// نام بیمه
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// کد بیمه
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// نشان می‌دهد که آیا بیمه فعال است یا خیر
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// مجموعه تعرفه‌های خدمات
    /// </summary>
    public virtual ICollection<ServiceTariff> ServiceTariffs { get; set; } = new List<ServiceTariff>();
}
