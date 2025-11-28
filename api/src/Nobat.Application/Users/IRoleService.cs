using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Users.Dto;
using Sieve.Models;

namespace Nobat.Application.Users;

/// <summary>
/// رابط سرویس نقش
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// دریافت نقش بر اساس شناسه
    /// </summary>
    Task<ApiResponse<RoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست نقش‌ها با فیلتر و مرتب‌سازی
    /// </summary>
    Task<ApiResponse<PagedResult<RoleDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد نقش جدید
    /// </summary>
    Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی نقش
    /// </summary>
    Task<ApiResponse<RoleDto>> UpdateAsync(UpdateRoleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف نقش
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
