using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.DatabaseChangeLogs.Dto;
using Nobat.Application.Schedules;
using Nobat.Domain.Entities.Common;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.DatabaseChangeLogs;

/// <summary>
/// سرویس لاگ تغییرات دیتابیس
/// این سرویس عملیات مربوط به مشاهده لاگ تغییرات دیتابیس را مدیریت می‌کند
/// </summary>
public class DatabaseChangeLogService : IDatabaseChangeLogService
{
    private readonly IRepository<DatabaseChangeLog> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<DatabaseChangeLogService> _logger;

    public DatabaseChangeLogService(
        IRepository<DatabaseChangeLog> repository,
        IUnitOfWork unitOfWork,
        ISieveProcessor sieveProcessor,
        ILogger<DatabaseChangeLogService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
    }

    public async Task<ApiResponse<DatabaseChangeLogDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var changeLog = await queryable
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (changeLog == null)
            {
                return ApiResponse<DatabaseChangeLogDto>.Error("لاگ تغییرات با شناسه مشخص شده یافت نشد", 404);
            }

            var changeLogDto = new DatabaseChangeLogDto
            {
                Id = changeLog.Id,
                UserId = changeLog.UserId,
                TableName = changeLog.TableName,
                RecordId = changeLog.RecordId,
                ChangeType = changeLog.ChangeType,
                ChangedColumns = changeLog.ChangedColumns,
                OldValues = changeLog.OldValues,
                NewValues = changeLog.NewValues,
                ChangeTime = changeLog.ChangeTime,
                IpAddress = changeLog.IpAddress,
                AdditionalData = changeLog.AdditionalData,
                CreatedAt = changeLog.CreatedAt
            };

            return ApiResponse<DatabaseChangeLogDto>.Success(changeLogDto, "لاگ تغییرات با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database change log by id: {Id}", id);
            return ApiResponse<DatabaseChangeLogDto>.Error("خطا در دریافت لاگ تغییرات", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<DatabaseChangeLogDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var query = queryable
                .Include(c => c.User)
                .OrderByDescending(c => c.ChangeTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var changeLogs = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<DatabaseChangeLogDto>
            {
                Items = changeLogs.Select(c => new DatabaseChangeLogDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    TableName = c.TableName,
                    RecordId = c.RecordId,
                    ChangeType = c.ChangeType,
                    ChangedColumns = c.ChangedColumns,
                    OldValues = c.OldValues,
                    NewValues = c.NewValues,
                    ChangeTime = c.ChangeTime,
                    IpAddress = c.IpAddress,
                    AdditionalData = c.AdditionalData,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<DatabaseChangeLogDto>>.Success(result, "لیست لاگ تغییرات با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all database change logs");
            return ApiResponse<PagedResult<DatabaseChangeLogDto>>.Error("خطا در دریافت لیست لاگ تغییرات", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<DatabaseChangeLogDto>>> GetByTableNameAsync(string tableName, SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var query = queryable
                .Where(c => c.TableName == tableName)
                .Include(c => c.User)
                .OrderByDescending(c => c.ChangeTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var changeLogs = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<DatabaseChangeLogDto>
            {
                Items = changeLogs.Select(c => new DatabaseChangeLogDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    TableName = c.TableName,
                    RecordId = c.RecordId,
                    ChangeType = c.ChangeType,
                    ChangedColumns = c.ChangedColumns,
                    OldValues = c.OldValues,
                    NewValues = c.NewValues,
                    ChangeTime = c.ChangeTime,
                    IpAddress = c.IpAddress,
                    AdditionalData = c.AdditionalData,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<DatabaseChangeLogDto>>.Success(result, "لیست لاگ تغییرات جدول با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database change logs by table name: {TableName}", tableName);
            return ApiResponse<PagedResult<DatabaseChangeLogDto>>.Error("خطا در دریافت لیست لاگ تغییرات جدول", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<DatabaseChangeLogDto>>> GetByChangeTypeAsync(string changeType, SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var query = queryable
                .Where(c => c.ChangeType == changeType)
                .Include(c => c.User)
                .OrderByDescending(c => c.ChangeTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var changeLogs = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<DatabaseChangeLogDto>
            {
                Items = changeLogs.Select(c => new DatabaseChangeLogDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    TableName = c.TableName,
                    RecordId = c.RecordId,
                    ChangeType = c.ChangeType,
                    ChangedColumns = c.ChangedColumns,
                    OldValues = c.OldValues,
                    NewValues = c.NewValues,
                    ChangeTime = c.ChangeTime,
                    IpAddress = c.IpAddress,
                    AdditionalData = c.AdditionalData,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<DatabaseChangeLogDto>>.Success(result, "لیست لاگ تغییرات بر اساس نوع با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database change logs by change type: {ChangeType}", changeType);
            return ApiResponse<PagedResult<DatabaseChangeLogDto>>.Error("خطا در دریافت لیست لاگ تغییرات بر اساس نوع", ex);
        }
    }
}
