using Nobat.Application.Common;
using Nobat.Application.DatabaseChangeLogs.Dto;
using Nobat.Application.Schedules;
using Sieve.Models;

namespace Nobat.Application.DatabaseChangeLogs;

/// <summary>
/// رابط سرویس لاگ تغییرات دیتابیس
/// </summary>
public interface IDatabaseChangeLogService
{
    /// <summary>
    /// دریافت لاگ تغییرات بر اساس شناسه
    /// </summary>
    Task<ApiResponse<DatabaseChangeLogDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست تمام لاگ تغییرات
    /// </summary>
    Task<ApiResponse<PagedResult<DatabaseChangeLogDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لاگ تغییرات بر اساس نام جدول
    /// </summary>
    Task<ApiResponse<PagedResult<DatabaseChangeLogDto>>> GetByTableNameAsync(string tableName, SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لاگ تغییرات بر اساس نوع تغییر
    /// </summary>
    Task<ApiResponse<PagedResult<DatabaseChangeLogDto>>> GetByChangeTypeAsync(string changeType, SieveModel sieveModel, CancellationToken cancellationToken = default);
}
