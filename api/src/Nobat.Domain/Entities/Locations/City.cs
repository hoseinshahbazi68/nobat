using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Locations;

/// <summary>
/// موجودیت شهر
/// این موجودیت اطلاعات شهرهای ایران را نگهداری می‌کند
/// </summary>
public class City : BaseEntity
{
    /// <summary>
    /// نام شهر
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// شناسه استان
    /// </summary>
    public int ProvinceId { get; set; }

    /// <summary>
    /// استان
    /// </summary>
    public virtual Province Province { get; set; } = null!;

    /// <summary>
    /// کد شهر (کد استاندارد ایران)
    /// </summary>
    public string? Code { get; set; }
}
