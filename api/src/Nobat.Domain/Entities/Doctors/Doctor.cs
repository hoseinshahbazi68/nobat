using Nobat.Domain.Common;
using Nobat.Domain.Entities.Services;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Entities.Appointments;
using Nobat.Domain.Enums;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت پزشک
/// این موجودیت اطلاعات پزشکان سیستم را نگهداری می‌کند
/// اطلاعات شخصی از User entity گرفته می‌شود
/// </summary>
public class Doctor : BaseEntity
{
    /// <summary>
    /// کد نظام پزشکی
    /// </summary>
    public string MedicalCode { get; set; } = string.Empty;

    /// <summary>
    /// پیشوند پزشک (دکتر، کارشناس، کارشناس ارشد)
    /// </summary>
    public DoctorPrefix Prefix { get; set; } = DoctorPrefix.None;

    /// <summary>
    /// درجه علمی
    /// </summary>
    public ScientificDegree ScientificDegree { get; set; } = ScientificDegree.None;

    /// <summary>
    /// شناسه کاربر (اختیاری - برای پزشکانی که می‌توانند وارد سیستم شوند)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// کاربر
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// اطلاعات ویزیت پزشک
    /// </summary>
    public virtual DoctorVisitInfo? VisitInfo { get; set; }

    /// <summary>
    /// مجموعه برنامه‌های پزشک
    /// </summary>
    public virtual ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();

    /// <summary>
    /// مجموعه تعرفه‌های خدمات پزشک
    /// </summary>
    public virtual ICollection<ServiceTariff> ServiceTariffs { get; set; } = new List<ServiceTariff>();

    /// <summary>
    /// مجموعه تخصص‌های پزشک (many-to-many)
    /// </summary>
    public virtual ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();

    /// <summary>
    /// مجموعه نوبت‌های پزشک
    /// </summary>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    /// <summary>
    /// مجموعه کلینیک‌های پزشک (many-to-many)
    /// </summary>
    public virtual ICollection<DoctorClinic> DoctorClinics { get; set; } = new List<DoctorClinic>();


    /// <summary>
    /// مجموعه علائم پزشکی مرتبط با پزشک (many-to-many)
    /// </summary>
    public virtual ICollection<DoctorMedicalCondition> DoctorMedicalConditions { get; set; } = new List<DoctorMedicalCondition>();
}
