using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Services.Dto;
using Sieve.Models;

namespace Nobat.Application.Services;

/// <summary>
/// رابط سرویس خدمت
/// </summary>
public interface IServiceService
{
    /// <summary>
    /// دریافت خدمت بر اساس شناسه
    /// </summary>
    Task<ApiResponse<ServiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست خدمات با فیلتر و مرتب‌سازی
    /// </summary>
    Task<ApiResponse<PagedResult<ServiceDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد خدمت جدید
    /// </summary>
    Task<ApiResponse<ServiceDto>> CreateAsync(CreateServiceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی خدمت
    /// </summary>
    Task<ApiResponse<ServiceDto>> UpdateAsync(UpdateServiceDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف خدمت
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
