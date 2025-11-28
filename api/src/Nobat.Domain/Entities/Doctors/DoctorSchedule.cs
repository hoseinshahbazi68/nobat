using Nobat.Domain.Common;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Entities.Appointments;
using Nobat.Domain.Entities.Clinics;
using Nobat.Domain.Entities.Services;
using Nobat.Domain.Enums;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت برنامه پزشک
/// این موجودیت برنامه زمانی پزشکان را نگهداری می‌کند
/// شامل تاریخ، شیفت، زمان شروع و پایان و وضعیت در دسترس بودن
/// برای مدیریت نوبت‌دهی استفاده می‌شود
/// </summary>
public class DoctorSchedule : BaseEntity
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
    /// شناسه شیفت
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// شیفت
    /// </summary>
    public virtual Shift Shift { get; set; } = null!;

    /// <summary>
    /// روز هفته
    /// </summary>
    public DayOfWeekModel DayOfWeek { get; set; }

    /// <summary>
    /// زمان شروع
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// زمان پایان
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// تعداد
    /// </summary>
    public int Count { get; set; } = 1;

    /// <summary>
    /// شناسه کلینیک (اختیاری - اگر null باشد، مطب شخصی پزشک است)
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// کلینیک (اختیاری)
    /// </summary>
    public virtual Clinic? Clinic { get; set; }

    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// خدمت
    /// </summary>
    public virtual Service Service { get; set; } = null!;

    /// <summary>
    /// مجموعه نوبت‌های این برنامه
    /// </summary>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
