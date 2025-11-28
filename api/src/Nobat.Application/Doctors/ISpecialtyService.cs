using Nobat.Application.Common;
using Nobat.Application.Doctors.Dto;
using Sieve.Models;

namespace Nobat.Application.Doctors;

/// <summary>
/// رابط سرویس تخصص
/// </summary>
public interface ISpecialtyService
{
    /// <summary>
    /// دریافت تخصص بر اساس شناسه
    /// </summary>
    Task<ApiResponse<SpecialtyDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست تخصص‌ها با فیلتر و مرتب‌سازی
    /// </summary>
    Task<ApiResponse<PagedResult<SpecialtyDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد تخصص جدید
    /// </summary>
    Task<ApiResponse<SpecialtyDto>> CreateAsync(CreateSpecialtyDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی تخصص
    /// </summary>
    Task<ApiResponse<SpecialtyDto>> UpdateAsync(UpdateSpecialtyDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف تخصص
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست علائم پزشکی مرتبط با تخصص
    /// </summary>
    Task<ApiResponse<List<SpecialtyMedicalConditionDto>>> GetMedicalConditionsAsync(int specialtyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن علائم پزشکی به تخصص
    /// </summary>
    Task<ApiResponse<SpecialtyMedicalConditionDto>> AddMedicalConditionAsync(int specialtyId, int medicalConditionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف علائم پزشکی از تخصص
    /// </summary>
    Task<ApiResponse> RemoveMedicalConditionAsync(int specialtyId, int medicalConditionId, CancellationToken cancellationToken = default);
}
