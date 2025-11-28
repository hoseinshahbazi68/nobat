using Nobat.Application.Common;
using Nobat.Application.Clinics.Dto;
using Sieve.Models;

namespace Nobat.Application.Clinics;

/// <summary>
/// رابط سرویس کلینیک
/// </summary>
public interface IClinicService
{
    /// <summary>
    /// دریافت کلینیک بر اساس شناسه
    /// </summary>
    Task<ApiResponse<ClinicDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست کلینیک‌ها با فیلتر و مرتب‌سازی
    /// </summary>
    Task<ApiResponse<PagedResult<ClinicDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد کلینیک جدید
    /// </summary>
    Task<ApiResponse<ClinicDto>> CreateAsync(CreateClinicDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی کلینیک
    /// </summary>
    Task<ApiResponse<ClinicDto>> UpdateAsync(UpdateClinicDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف کلینیک
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست ساده کلینیک‌ها (فقط نام و شناسه)
    /// </summary>
    Task<ApiResponse<List<ClinicSimpleDto>>> GetSimpleListAsync(string? searchTerm = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست کاربران کلینیک
    /// </summary>
    Task<ApiResponse<List<Users.Dto.UserDto>>> GetClinicUsersAsync(int clinicId, CancellationToken cancellationToken = default);
}
