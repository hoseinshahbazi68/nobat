using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت تخصص
/// </summary>
public class Specialty : BaseEntity
{
    /// <summary>
    /// نام تخصص
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات تخصص
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// مجموعه علائم پزشکی مرتبط با تخصص (many-to-many)
    /// </summary>
    public virtual ICollection<SpecialtyMedicalCondition> SpecialtyMedicalConditions { get; set; } = new List<SpecialtyMedicalCondition>();
}
