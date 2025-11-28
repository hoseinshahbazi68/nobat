using Nobat.Application.Common;
using Nobat.Application.Doctors.Dto;
using Nobat.Application.Schedules;
using Sieve.Models;

namespace Nobat.Application.Doctors;

/// <summary>
/// رابط سرویس پزشک
/// </summary>
public interface IDoctorService
{
    /// <summary>
    /// دریافت پزشک بر اساس شناسه
    /// </summary>
    Task<ApiResponse<DoctorDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست پزشکان با فیلتر و مرتب‌سازی
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="clinicId">شناسه مرکز (اختیاری - برای فیلتر بر اساس مرکز)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task<ApiResponse<PagedResult<DoctorListDto>>> GetAllAsync(SieveModel sieveModel, int? clinicId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد پزشک جدید
    /// </summary>
    Task<ApiResponse<DoctorDto>> CreateAsync(CreateDoctorDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد پزشک جدید با فایل
    /// </summary>
    Task<ApiResponse<DoctorDto>> CreateWithFileAsync(CreateDoctorWithFileDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی پزشک
    /// </summary>
    Task<ApiResponse<DoctorDto>> UpdateAsync(UpdateDoctorDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی پزشک با فایل
    /// </summary>
    Task<ApiResponse<DoctorDto>> UpdateWithFileAsync(UpdateDoctorWithFileDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف پزشک
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// جستجوی پزشکان بر اساس نام پزشک یا نام کلینیک
    /// </summary>
    Task<ApiResponse<PagedResult<DoctorDto>>> SearchAsync(
        string? query = null,
        string? clinicName = null,
        string? doctorName = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// جستجوی پزشک بر اساس کد نظام پزشکی
    /// </summary>
    Task<ApiResponse<DoctorDto>> GetByMedicalCodeAsync(string medicalCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// جستجوی پزشکان بر اساس علائم پزشکی
    /// </summary>
    Task<ApiResponse<PagedResult<DoctorDto>>> SearchByMedicalConditionAsync(
        string medicalConditionName,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
