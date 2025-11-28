using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Users.Dto;
using Sieve.Models;

namespace Nobat.Application.Users;

/// <summary>
/// رابط سرویس لاگ فعالیت کاربر
/// </summary>
public interface IUserActivityLogService
{
    /// <summary>
    /// دریافت لاگ فعالیت کاربر بر اساس شناسه
    /// </summary>
    Task<ApiResponse<UserActivityLogDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست تمام لاگ فعالیت‌های کاربران
    /// </summary>
    Task<ApiResponse<PagedResult<UserActivityLogDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لاگ فعالیت‌های یک کاربر خاص
    /// </summary>
    Task<ApiResponse<PagedResult<UserActivityLogDto>>> GetByUserIdAsync(int userId, SieveModel sieveModel, CancellationToken cancellationToken = default);
}
