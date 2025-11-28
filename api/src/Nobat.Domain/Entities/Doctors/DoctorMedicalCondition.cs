using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت رابطه پزشک و علائم پزشکی
/// این موجودیت رابطه many-to-many بین Doctor و MedicalCondition را برقرار می‌کند
/// </summary>
public class DoctorMedicalCondition : BaseEntity
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// پزشک
    /// </summary>
    public virtual Doctor Doctor { get; set; } = null!;

    /// <summary>
    /// شناسه علائم پزشکی
    /// </summary>
    public int MedicalConditionId { get; set; }

    /// <summary>
    /// علائم پزشکی
    /// </summary>
    public virtual MedicalCondition MedicalCondition { get; set; } = null!;
}
