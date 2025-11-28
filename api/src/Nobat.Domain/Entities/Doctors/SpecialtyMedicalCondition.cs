using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت رابطه تخصص و علائم پزشکی
/// این موجودیت رابطه many-to-many بین Specialty و MedicalCondition را برقرار می‌کند
/// </summary>
public class SpecialtyMedicalCondition : BaseEntity
{
    /// <summary>
    /// شناسه تخصص
    /// </summary>
    public int SpecialtyId { get; set; }

    /// <summary>
    /// تخصص
    /// </summary>
    public virtual Specialty Specialty { get; set; } = null!;

    /// <summary>
    /// شناسه علائم پزشکی
    /// </summary>
    public int MedicalConditionId { get; set; }

    /// <summary>
    /// علائم پزشکی
    /// </summary>
    public virtual MedicalCondition MedicalCondition { get; set; } = null!;
}
