using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Clinics.Dto;
using Nobat.Application.Repositories;
using Nobat.Application.Users.Dto;
using Nobat.Domain.Entities.Clinics;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Clinics;

/// <summary>
/// سرویس کلینیک
/// این سرویس عملیات CRUD مربوط به کلینیک‌ها را مدیریت می‌کند
/// شامل دریافت، ایجاد، به‌روزرسانی و حذف کلینیک‌ها
/// از Sieve برای فیلتر و مرتب‌سازی استفاده می‌کند
/// </summary>
public class ClinicService : IClinicService
{
    private readonly IRepository<Clinic> _repository;
    private readonly IRepository<ClinicUser> _clinicUserRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<ClinicService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// سازنده سرویس کلینیک
    /// </summary>
    public ClinicService(
        IRepository<Clinic> repository,
        IRepository<ClinicUser> clinicUserRepository,
        IRepository<User> userRepository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<ClinicService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _clinicUserRepository = clinicUserRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// دریافت کلینیک بر اساس شناسه
    /// </summary>
    public async Task<ApiResponse<ClinicDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            var clinic = await query
                .Include(c => c.City)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (clinic == null)
            {
                return ApiResponse<ClinicDto>.Error("کلینیک با شناسه مشخص شده یافت نشد", 404);
            }

            var clinicDto = _mapper.Map<ClinicDto>(clinic);
            return ApiResponse<ClinicDto>.Success(clinicDto, "کلینیک با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting clinic by id: {Id}", id);
            return ApiResponse<ClinicDto>.Error("خطا در دریافت کلینیک", ex);
        }
    }

    /// <summary>
    /// دریافت لیست کلینیک‌ها با فیلتر و مرتب‌سازی
    /// </summary>
    public async Task<ApiResponse<PagedResult<ClinicDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            query = query.Include(c => c.City);

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var clinics = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<ClinicDto>
            {
                Items = _mapper.Map<List<ClinicDto>>(clinics),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<ClinicDto>>.Success(result, "لیست کلینیک‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all clinics");
            return ApiResponse<PagedResult<ClinicDto>>.Error("خطا در دریافت لیست کلینیک‌ها", ex);
        }
    }

    /// <summary>
    /// ایجاد کلینیک جدید
    /// </summary>
    public async Task<ApiResponse<ClinicDto>> CreateAsync(CreateClinicDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var clinic = _mapper.Map<Clinic>(dto);
            await _repository.AddAsync(clinic, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Clinic created with ID: {ClinicId}", clinic.Id);
            var clinicDto = _mapper.Map<ClinicDto>(clinic);
            return ApiResponse<ClinicDto>.Success(clinicDto, "کلینیک جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating clinic");
            return ApiResponse<ClinicDto>.Error("خطا در ایجاد کلینیک", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی کلینیک
    /// </summary>
    public async Task<ApiResponse<ClinicDto>> UpdateAsync(UpdateClinicDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var clinic = await query
                .Include(c => c.City)
                .FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

            if (clinic == null)
            {
                return ApiResponse<ClinicDto>.Error($"کلینیک با شناسه {dto.Id} یافت نشد", 404);
            }

            _mapper.Map(dto, clinic);
            await _repository.UpdateAsync(clinic, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Clinic updated with ID: {ClinicId}", clinic.Id);
            var clinicDto = _mapper.Map<ClinicDto>(clinic);
            return ApiResponse<ClinicDto>.Success(clinicDto, "کلینیک با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating clinic");
            return ApiResponse<ClinicDto>.Error("خطا در به‌روزرسانی کلینیک", ex);
        }
    }

    /// <summary>
    /// حذف کلینیک
    /// </summary>
    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Clinic deleted with ID: {ClinicId}", id);
            return ApiResponse.Success("کلینیک با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting clinic");
            return ApiResponse.Error("خطا در حذف کلینیک", ex);
        }
    }

    /// <summary>
    /// دریافت لیست ساده کلینیک‌ها (فقط نام و شناسه)
    /// </summary>
    public async Task<ApiResponse<List<ClinicSimpleDto>>> GetSimpleListAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);

            // فیلتر بر اساس نام اگر searchTerm ارائه شده باشد
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm));
            }

            var clinics = await query.Select(c => new ClinicSimpleDto
            {
                Id = c.Id,
                Name = c.Name
            }).OrderBy(c => c.Name).ToListAsync(cancellationToken);

            return ApiResponse<List<ClinicSimpleDto>>.Success(clinics, "لیست کلینیک‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting simple clinic list");
            return ApiResponse<List<ClinicSimpleDto>>.Error("خطا در دریافت لیست کلینیک‌ها", ex);
        }
    }

    /// <summary>
    /// دریافت لیست کاربران کلینیک
    /// </summary>
    public async Task<ApiResponse<List<UserDto>>> GetClinicUsersAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var clinic = await _repository.GetByIdAsync(clinicId, cancellationToken);
            if (clinic == null)
            {
                return ApiResponse<List<UserDto>>.Error("کلینیک یافت نشد", 404);
            }

            var clinicUsers = await _clinicUserRepository.FindAsync(cu => cu.ClinicId == clinicId, cancellationToken);
            var userIds = clinicUsers.Select(cu => cu.UserId).ToList();

            var users = new List<UserDto>();
            foreach (var userId in userIds)
            {
                var user = await _userRepository.GetWithRolesAsync(userId, cancellationToken);
                if (user != null)
                {
                    var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
                    var userDto = _mapper.Map<UserDto>(user);
                    userDto.Roles = roles;
                    userDto.CityName = user.City?.Name;
                    userDto.ProvinceName = user.City?.Province?.Name;
                    users.Add(userDto);
                }
            }

            return ApiResponse<List<UserDto>>.Success(users, "لیست کاربران کلینیک با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting clinic users for clinicId: {ClinicId}", clinicId);
            return ApiResponse<List<UserDto>>.Error("خطا در دریافت لیست کاربران کلینیک", ex);
        }
    }
}
