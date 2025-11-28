namespace Nobat.Application.Appointments.Dto;

/// <summary>
/// DTO ایجاد نوبت
/// </summary>
public class CreateAppointmentDto
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// شناسه برنامه پزشک
    /// </summary>
    public int DoctorScheduleId { get; set; }

    /// <summary>
    /// تاریخ و زمان نوبت
    /// </summary>
    public DateTime AppointmentDateTime { get; set; }

    /// <summary>
    /// تاریخ و زمان انقضا
    /// </summary>
    public DateTime ExpireDateTime { get; set; }

    /// <summary>
    /// زمان شروع (به صورت string مانند "08:00")
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// زمان پایان (به صورت string مانند "12:00")
    /// </summary>
    public string EndTime { get; set; } = string.Empty;
}
