using Nobat.Domain.Common;
using Nobat.Domain.Entities.Clinics;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت رابطه پزشک و کلینیک
/// این موجودیت رابطه many-to-many بین پزشکان و کلینیک‌ها را برقرار می‌کند
/// </summary>
public class DoctorClinic : BaseEntity
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
    /// شناسه کلینیک
    /// </summary>
    public int ClinicId { get; set; }

    /// <summary>
    /// کلینیک
    /// </summary>
    public virtual Clinic Clinic { get; set; } = null!;

    /// <summary>
    /// نشان می‌دهد که آیا پزشک در این کلینیک فعال است یا خیر
    /// </summary>
    public bool IsActive { get; set; } = true;
}
