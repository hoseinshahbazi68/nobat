using Nobat.Application.Common;
using Nobat.Application.QueryLogs.Dto;
using Nobat.Application.Schedules;
using Sieve.Models;

namespace Nobat.Application.QueryLogs;

/// <summary>
/// رابط سرویس لاگ کوئری
/// </summary>
public interface IQueryLogService
{
    /// <summary>
    /// دریافت لاگ کوئری بر اساس شناسه
    /// </summary>
    Task<ApiResponse<QueryLogDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست لاگ کوئری‌های سنگین
    /// </summary>
    Task<ApiResponse<PagedResult<QueryLogDto>>> GetHeavyQueriesAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست تمام لاگ کوئری‌ها
    /// </summary>
    Task<ApiResponse<PagedResult<QueryLogDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت آمار کوئری‌های سنگین
    /// </summary>
    Task<ApiResponse<QueryLogStatisticsDto>> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف لاگ‌های قدیمی‌تر از تعداد روز مشخص
    /// </summary>
    Task<ApiResponse<int>> DeleteOldLogsAsync(int daysToKeep, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO آمار لاگ کوئری
/// </summary>
public class QueryLogStatisticsDto
{
    /// <summary>
    /// تعداد کل کوئری‌های سنگین
    /// </summary>
    public int TotalHeavyQueries { get; set; }

    /// <summary>
    /// متوسط زمان اجرای کوئری‌های سنگین (میلی‌ثانیه)
    /// </summary>
    public double AverageExecutionTimeMs { get; set; }

    /// <summary>
    /// بیشترین زمان اجرا (میلی‌ثانیه)
    /// </summary>
    public long MaxExecutionTimeMs { get; set; }

    /// <summary>
    /// کمترین زمان اجرای کوئری‌های سنگین (میلی‌ثانیه)
    /// </summary>
    public long MinExecutionTimeMs { get; set; }

    /// <summary>
    /// تعداد کوئری‌های با خطا
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// آمار بر اساس نوع دستور
    /// </summary>
    public Dictionary<string, int> CommandTypeCounts { get; set; } = new();

    /// <summary>
    /// آمار بر اساس جداول استفاده شده
    /// </summary>
    public Dictionary<string, int> TableUsageCounts { get; set; } = new();
}
