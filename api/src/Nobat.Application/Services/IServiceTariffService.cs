using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Services.Dto;
using Sieve.Models;

namespace Nobat.Application.Services;

/// <summary>
/// رابط سرویس تعرفه خدمت
/// </summary>
public interface IServiceTariffService
{
    /// <summary>
    /// دریافت تعرفه خدمت بر اساس شناسه
    /// </summary>
    Task<ApiResponse<ServiceTariffDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست تعرفه‌های خدمات با فیلتر و مرتب‌سازی
    /// </summary>
    Task<ApiResponse<PagedResult<ServiceTariffDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد تعرفه خدمت جدید
    /// </summary>
    Task<ApiResponse<ServiceTariffDto>> CreateAsync(CreateServiceTariffDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی تعرفه خدمت
    /// </summary>
    Task<ApiResponse<ServiceTariffDto>> UpdateAsync(UpdateServiceTariffDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف تعرفه خدمت
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
