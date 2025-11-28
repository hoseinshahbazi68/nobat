using Nobat.Application.Common;
using Nobat.Application.Doctors.Dto;

namespace Nobat.Application.Doctors;

/// <summary>
/// اینترفیس سرویس اطلاعات ویزیت پزشک
/// </summary>
public interface IDoctorVisitInfoService
{
    /// <summary>
    /// دریافت اطلاعات ویزیت بر اساس شناسه
    /// </summary>
    Task<ApiResponse<DoctorVisitInfoDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت اطلاعات ویزیت بر اساس شناسه پزشک
    /// </summary>
    Task<ApiResponse<DoctorVisitInfoDto>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست اطلاعات ویزیت
    /// </summary>
    Task<ApiResponse<PagedResult<DoctorVisitInfoDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد اطلاعات ویزیت جدید
    /// </summary>
    Task<ApiResponse<DoctorVisitInfoDto>> CreateAsync(CreateDoctorVisitInfoDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی اطلاعات ویزیت
    /// </summary>
    Task<ApiResponse<DoctorVisitInfoDto>> UpdateAsync(UpdateDoctorVisitInfoDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف اطلاعات ویزیت
    /// </summary>
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
