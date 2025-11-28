using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.QueryLogs.Dto;
using Nobat.Application.Schedules;
using Nobat.Domain.Entities.Common;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.QueryLogs;

/// <summary>
/// سرویس لاگ کوئری
/// این سرویس عملیات مربوط به لاگ‌گیری و مشاهده کوئری‌های سنگین را مدیریت می‌کند
/// </summary>
public class QueryLogService : IQueryLogService
{
    private readonly IRepository<QueryLog> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<QueryLogService> _logger;

    public QueryLogService(
        IRepository<QueryLog> repository,
        IUnitOfWork unitOfWork,
        ISieveProcessor sieveProcessor,
        ILogger<QueryLogService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
    }

    public async Task<ApiResponse<QueryLogDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            var queryLog = await queryable
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

            if (queryLog == null)
            {
                return ApiResponse<QueryLogDto>.Error("لاگ کوئری با شناسه مشخص شده یافت نشد", 404);
            }

            var queryLogDto = new QueryLogDto
            {
                Id = queryLog.Id,
                UserId = queryLog.UserId,
                QueryText = queryLog.QueryText,
                Parameters = queryLog.Parameters,
                ExecutionTimeMs = queryLog.ExecutionTimeMs,
                ExecutionTime = queryLog.ExecutionTime,
                CommandType = queryLog.CommandType,
                TablesUsed = queryLog.TablesUsed,
                IpAddress = queryLog.IpAddress,
                ControllerAction = queryLog.ControllerAction,
                IsHeavy = queryLog.IsHeavy,
                ErrorMessage = queryLog.ErrorMessage,
                CreatedAt = queryLog.CreatedAt
            };

            return ApiResponse<QueryLogDto>.Success(queryLogDto, "لاگ کوئری با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query log by id: {Id}", id);
            return ApiResponse<QueryLogDto>.Error("خطا در دریافت لاگ کوئری", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<QueryLogDto>>> GetHeavyQueriesAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var query = queryable
                .Where(q => q.IsHeavy)
                .Include(q => q.User)
                .OrderByDescending(q => q.ExecutionTimeMs);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var queryLogs = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<QueryLogDto>
            {
                Items = queryLogs.Select(q => new QueryLogDto
                {
                    Id = q.Id,
                    UserId = q.UserId,
                    QueryText = q.QueryText,
                    Parameters = q.Parameters,
                    ExecutionTimeMs = q.ExecutionTimeMs,
                    ExecutionTime = q.ExecutionTime,
                    CommandType = q.CommandType,
                    TablesUsed = q.TablesUsed,
                    IpAddress = q.IpAddress,
                    ControllerAction = q.ControllerAction,
                    IsHeavy = q.IsHeavy,
                    ErrorMessage = q.ErrorMessage,
                    CreatedAt = q.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<QueryLogDto>>.Success(result, "لیست کوئری‌های سنگین با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting heavy queries");
            return ApiResponse<PagedResult<QueryLogDto>>.Error("خطا در دریافت لیست کوئری‌های سنگین", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<QueryLogDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var query = queryable
                .Include(q => q.User)
                .OrderByDescending(q => q.ExecutionTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var queryLogs = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<QueryLogDto>
            {
                Items = queryLogs.Select(q => new QueryLogDto
                {
                    Id = q.Id,
                    UserId = q.UserId,
                    QueryText = q.QueryText,
                    Parameters = q.Parameters,
                    ExecutionTimeMs = q.ExecutionTimeMs,
                    ExecutionTime = q.ExecutionTime,
                    CommandType = q.CommandType,
                    TablesUsed = q.TablesUsed,
                    IpAddress = q.IpAddress,
                    ControllerAction = q.ControllerAction,
                    IsHeavy = q.IsHeavy,
                    ErrorMessage = q.ErrorMessage,
                    CreatedAt = q.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<QueryLogDto>>.Success(result, "لیست لاگ کوئری‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all query logs");
            return ApiResponse<PagedResult<QueryLogDto>>.Error("خطا در دریافت لیست لاگ کوئری‌ها", ex);
        }
    }

    public async Task<ApiResponse<QueryLogStatisticsDto>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var heavyQueries = await queryable
                .Where(q => q.IsHeavy)
                .ToListAsync(cancellationToken);

            var allQueryable = await _repository.GetQueryableAsync(cancellationToken);
            var errorCount = await allQueryable
                .Where(q => !string.IsNullOrEmpty(q.ErrorMessage))
                .CountAsync(cancellationToken);

            var statistics = new QueryLogStatisticsDto
            {
                TotalHeavyQueries = heavyQueries.Count,
                ErrorCount = errorCount
            };

            if (heavyQueries.Any())
            {
                statistics.AverageExecutionTimeMs = heavyQueries.Average(q => q.ExecutionTimeMs);
                statistics.MaxExecutionTimeMs = heavyQueries.Max(q => q.ExecutionTimeMs);
                statistics.MinExecutionTimeMs = heavyQueries.Min(q => q.ExecutionTimeMs);

                // آمار بر اساس نوع دستور
                statistics.CommandTypeCounts = heavyQueries
                    .Where(q => !string.IsNullOrEmpty(q.CommandType))
                    .GroupBy(q => q.CommandType!)
                    .ToDictionary(g => g.Key, g => g.Count());

                // آمار بر اساس جداول استفاده شده
                var tableUsage = heavyQueries
                    .Where(q => !string.IsNullOrEmpty(q.TablesUsed))
                    .SelectMany(q => q.TablesUsed!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(t => t.Trim())
                    .GroupBy(t => t)
                    .ToDictionary(g => g.Key, g => g.Count());

                statistics.TableUsageCounts = tableUsage;
            }
            else
            {
                statistics.MinExecutionTimeMs = 0;
            }

            return ApiResponse<QueryLogStatisticsDto>.Success(statistics, "آمار کوئری‌های سنگین با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return ApiResponse<QueryLogStatisticsDto>.Error("خطا در دریافت آمار کوئری‌های سنگین", ex);
        }
    }

    public async Task<ApiResponse<int>> DeleteOldLogsAsync(int daysToKeep, CancellationToken cancellationToken = default)
    {
        try
        {
            if (daysToKeep < 1)
            {
                return ApiResponse<int>.Error("تعداد روز باید بیشتر از 0 باشد", 400, "مقدار daysToKeep باید عدد مثبت باشد");
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var oldLogs = await queryable
                .Where(q => q.CreatedAt < cutoffDate && !q.IsHeavy)
                .ToListAsync(cancellationToken);

            foreach (var log in oldLogs)
            {
                await _repository.DeleteAsync(log.Id, cancellationToken);
            }

            var deletedCount = await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"حذف {deletedCount} لاگ قدیمی انجام شد.");

            return ApiResponse<int>.Success(deletedCount, $"{deletedCount} لاگ قدیمی با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old logs");
            return ApiResponse<int>.Error("خطا در حذف لاگ‌های قدیمی", ex);
        }
    }
}
