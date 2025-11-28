namespace Nobat.Application.Appointments.Dto;

/// <summary>
/// DTO تعداد نوبت‌ها برای یک تاریخ
/// </summary>
public class AppointmentCountDto
{
    /// <summary>
    /// تاریخ
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// تعداد کل نوبت‌ها
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// تعداد نوبت‌های رزرو شده
    /// </summary>
    public int BookedCount { get; set; }

    /// <summary>
    /// تعداد نوبت‌های آزاد
    /// </summary>
    public int AvailableCount { get; set; }
}
