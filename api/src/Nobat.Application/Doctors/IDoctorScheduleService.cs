using Nobat.Application.Common;
using Nobat.Application.Doctors.Dto;
using Nobat.Application.Schedules;

namespace Nobat.Application.Doctors;

/// <summary>
/// رابط سرویس برنامه پزشک
/// </summary>
public interface IDoctorScheduleService
{
    /// <summary>
    /// دریافت برنامه پزشک بر اساس شناسه
    /// </summary>
    Task<ApiResponse<DoctorScheduleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست برنامه‌های پزشک
    /// </summary>
    Task<ApiResponse<PagedResult<DoctorScheduleDto>>> GetAllAsync(Sieve.Models.SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت برنامه هفتگی پزشک
    /// </summary>
    Task<ApiResponse<WeeklyScheduleDto>> GetWeeklyScheduleAsync(int doctorId, string weekStartDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد برنامه پزشک جدید
    /// </summary>
    Task<ApiResponse<DoctorScheduleDto>> CreateAsync(CreateDoctorScheduleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی برنامه پزشک
    /// </summary>
    Task<ApiResponse<DoctorScheduleDto>> UpdateAsync(UpdateDoctorScheduleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف برنامه پزشک
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// تولید برنامه پزشک برای بازه زمانی مشخص
    /// </summary>
    Task<ApiResponse<List<DoctorScheduleDto>>> GenerateScheduleAsync(GenerateScheduleDto dto, CancellationToken cancellationToken = default);
}
