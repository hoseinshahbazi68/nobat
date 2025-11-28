using Nobat.Application.Common;
using Nobat.Application.Doctors.Dto;
using Sieve.Models;

namespace Nobat.Application.Doctors;

/// <summary>
/// اینترفیس سرویس مدیریت علائم پزشکی
/// </summary>
public interface IMedicalConditionService
{
    /// <summary>
    /// دریافت علائم بر اساس شناسه
    /// </summary>
    Task<ApiResponse<MedicalConditionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست علائم با فیلتر و مرتب‌سازی
    /// </summary>
    Task<ApiResponse<PagedResult<MedicalConditionDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد علائم جدید
    /// </summary>
    Task<ApiResponse<MedicalConditionDto>> CreateAsync(CreateMedicalConditionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی علائم
    /// </summary>
    Task<ApiResponse<MedicalConditionDto>> UpdateAsync(UpdateMedicalConditionDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف علائم
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// جستجوی علائم بر اساس نام
    /// </summary>
    Task<ApiResponse<List<MedicalConditionDto>>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
