using Nobat.Application.Common;
using Nobat.Domain.Entities.Appointments;

namespace Nobat.Application.Appointments;

/// <summary>
/// رابط سرویس نوبت
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// ایجاد نوبت جدید
    /// </summary>
    Task<ApiResponse<Appointment>> CreateAsync(Appointment appointment, CancellationToken cancellationToken = default);

    /// <summary>
    /// بررسی وجود نوبت برای یک برنامه و تاریخ مشخص
    /// </summary>
    Task<bool> ExistsAsync(int doctorScheduleId, DateTime appointmentDateTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید خودکار نوبت‌ها برای یک بازه زمانی
    /// </summary>
    Task<ApiResponse<int>> GenerateAppointmentsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت تعداد نوبت‌ها برای یک بازه تاریخ و پزشک مشخص
    /// </summary>
    Task<ApiResponse<List<Dto.AppointmentCountDto>>> GetAppointmentCountsAsync(int doctorId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
