using Nobat.Domain.Common;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Locations;

namespace Nobat.Domain.Entities.Clinics;

/// <summary>
/// موجودیت کلینیک
/// این موجودیت اطلاعات کلینیک‌ها را نگهداری می‌کند
/// </summary>
public class Clinic : BaseEntity
{
    /// <summary>
    /// نام کلینیک
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// آدرس کلینیک
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// شماره تلفن کلینیک
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// ایمیل کلینیک
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// شهر
    /// </summary>
    public virtual City? City { get; set; }

    /// <summary>
    /// نشان می‌دهد که آیا کلینیک فعال است یا خیر
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// تعداد روز تولید نوبت‌دهی
    /// تعداد روزهایی که نوبت‌ها به صورت خودکار تولید می‌شوند
    /// </summary>
    public int? AppointmentGenerationDays { get; set; }

    /// <summary>
    /// مجموعه پزشکان کلینیک (many-to-many)
    /// </summary>
    public virtual ICollection<DoctorClinic> DoctorClinics { get; set; } = new List<DoctorClinic>();

    /// <summary>
    /// مجموعه مدیران کلینیک (many-to-many)
    /// </summary>
    public virtual ICollection<ClinicUser> ClinicUsers { get; set; } = new List<ClinicUser>();
}
