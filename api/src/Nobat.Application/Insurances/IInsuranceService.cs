using Nobat.Application.Common;
using Nobat.Application.Insurances.Dto;
using Nobat.Application.Schedules;
using Sieve.Models;

namespace Nobat.Application.Insurances;

/// <summary>
/// رابط سرویس بیمه
/// </summary>
public interface IInsuranceService
{
    /// <summary>
    /// دریافت بیمه بر اساس شناسه
    /// </summary>
    Task<ApiResponse<InsuranceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست بیمه‌ها با فیلتر و مرتب‌سازی
    /// </summary>
    Task<ApiResponse<PagedResult<InsuranceDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد بیمه جدید
    /// </summary>
    Task<ApiResponse<InsuranceDto>> CreateAsync(CreateInsuranceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی بیمه
    /// </summary>
    Task<ApiResponse<InsuranceDto>> UpdateAsync(UpdateInsuranceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف بیمه
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
