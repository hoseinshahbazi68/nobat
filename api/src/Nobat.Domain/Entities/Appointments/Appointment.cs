using Nobat.Domain.Common;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Users;

namespace Nobat.Domain.Entities.Appointments;

/// <summary>
/// موجودیت نوبت
/// این موجودیت اطلاعات نوبت‌های رزرو شده را نگهداری می‌کند
/// </summary>
public class Appointment : BaseEntity
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
    /// شناسه برنامه پزشک
    /// </summary>
    public int DoctorScheduleId { get; set; }

    /// <summary>
    /// برنامه پزشک
    /// </summary>
    public virtual DoctorSchedule DoctorSchedule { get; set; } = null!;

    /// <summary>
    /// تاریخ و زمان نوبت
    /// </summary>
    public DateTime AppointmentDateTime { get; set; }

    /// <summary>
    /// تاریخ و زمان نوبت
    /// </summary>
    public DateTime ExpireDateTime { get; set; }

    /// <summary>
    /// زمان شروع
    /// </summary>
    public TimeSpan StartTime { get; set; }
    /// <summary>
    /// زمان پایان
    /// </summary>
    public TimeSpan EndTime { get; set; }
    /// <summary>
    /// وضعیت نوبت (رزرو شده، لغو شده، انجام شده)
    /// </summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
}

/// <summary>
/// وضعیت نوبت
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// رزرو شده
    /// </summary>
    Booked = 1,

    /// <summary>
    /// لغو شده
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// انجام شده
    /// </summary>
    Completed = 3
}
