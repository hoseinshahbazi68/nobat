using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Users.Dto;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Users;

/// <summary>
/// سرویس لاگ فعالیت کاربر
/// این سرویس عملیات مربوط به مشاهده لاگ فعالیت‌های کاربران را مدیریت می‌کند
/// </summary>
public class UserActivityLogService : IUserActivityLogService
{
    private readonly IRepository<UserActivityLog> _repository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<UserActivityLogService> _logger;

    public UserActivityLogService(
        IRepository<UserActivityLog> repository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<UserActivityLogService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
    }

    public async Task<ApiResponse<UserActivityLogDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var activityLog = await queryable
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (activityLog == null)
            {
                return ApiResponse<UserActivityLogDto>.Error("لاگ فعالیت با شناسه مشخص شده یافت نشد", 404);
            }

            var activityLogDto = _mapper.Map<UserActivityLogDto>(activityLog);
            if (activityLog.User != null)
            {
                activityLogDto.UserFullName = $"{activityLog.User.FirstName} {activityLog.User.LastName}".Trim();
            }

            return ApiResponse<UserActivityLogDto>.Success(activityLogDto, "لاگ فعالیت با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity log by id: {Id}", id);
            return ApiResponse<UserActivityLogDto>.Error("خطا در دریافت لاگ فعالیت", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<UserActivityLogDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var query = queryable
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var activityLogs = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<UserActivityLogDto>
            {
                Items = activityLogs.Select(a =>
                {
                    var dto = _mapper.Map<UserActivityLogDto>(a);
                    if (a.User != null)
                    {
                        dto.UserFullName = $"{a.User.FirstName} {a.User.LastName}".Trim();
                    }
                    return dto;
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<UserActivityLogDto>>.Success(result, "لیست لاگ فعالیت‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all user activity logs");
            return ApiResponse<PagedResult<UserActivityLogDto>>.Error("خطا در دریافت لیست لاگ فعالیت‌ها", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<UserActivityLogDto>>> GetByUserIdAsync(int userId, SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryable = await _repository.GetQueryableAsync(cancellationToken);
            var query = queryable
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .OrderByDescending(a => a.ActivityTime);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var activityLogs = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<UserActivityLogDto>
            {
                Items = activityLogs.Select(a =>
                {
                    var dto = _mapper.Map<UserActivityLogDto>(a);
                    if (a.User != null)
                    {
                        dto.UserFullName = $"{a.User.FirstName} {a.User.LastName}".Trim();
                    }
                    return dto;
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<UserActivityLogDto>>.Success(result, "لیست لاگ فعالیت‌های کاربر با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity logs by user id: {UserId}", userId);
            return ApiResponse<PagedResult<UserActivityLogDto>>.Error("خطا در دریافت لیست لاگ فعالیت‌های کاربر", ex);
        }
    }
}
