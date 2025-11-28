using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت رابطه پزشک و تخصص
/// این موجودیت رابطه many-to-many بین Doctor و Specialty را برقرار می‌کند
/// </summary>
public class DoctorSpecialty : BaseEntity
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
    /// شناسه تخصص
    /// </summary>
    public int SpecialtyId { get; set; }

    /// <summary>
    /// تخصص
    /// </summary>
    public virtual Specialty Specialty { get; set; } = null!;

    /// <summary>
    /// ترتیب نمایش تخصص (برای sort)
    /// </summary>
    public int SortOrder { get; set; } = 0;
}
