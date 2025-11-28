using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Locations;

/// <summary>
/// موجودیت استان
/// این موجودیت اطلاعات استان‌های ایران را نگهداری می‌کند
/// </summary>
public class Province : BaseEntity
{
    /// <summary>
    /// نام استان
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// کد استان (کد استاندارد ایران)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// مجموعه شهرهای استان
    /// </summary>
    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
