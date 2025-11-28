using AutoMapper;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Repositories;
using Nobat.Application.Schedules;
using Nobat.Application.Users.Dto;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Users;

/// <summary>
/// سرویس مدیریت نقش‌ها
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<RoleService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// سازنده سرویس نقش
    /// </summary>
    public RoleService(
        IRepository<Role> roleRepository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<RoleService> logger,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// دریافت نقش بر اساس شناسه
    /// </summary>
    public async Task<ApiResponse<RoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
            if (role == null)
            {
                return ApiResponse<RoleDto>.Error("نقش با شناسه مشخص شده یافت نشد", 404);
            }

            var roleDto = _mapper.Map<RoleDto>(role);
            return ApiResponse<RoleDto>.Success(roleDto, "نقش با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by id: {Id}", id);
            return ApiResponse<RoleDto>.Error("خطا در دریافت نقش", ex);
        }
    }

    /// <summary>
    /// دریافت لیست نقش‌ها با فیلتر و مرتب‌سازی
    /// </summary>
    public async Task<ApiResponse<PagedResult<RoleDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _roleRepository.GetQueryableAsync(cancellationToken);

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var roles = filteredQuery.ToList();

            var result = new PagedResult<RoleDto>
            {
                Items = _mapper.Map<List<RoleDto>>(roles),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<RoleDto>>.Success(result, "لیست نقش‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return ApiResponse<PagedResult<RoleDto>>.Error("خطا در دریافت لیست نقش‌ها", ex);
        }
    }

    /// <summary>
    /// ایجاد نقش جدید
    /// </summary>
    public async Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی تکراری نبودن نام نقش
            var existingRole = await _roleRepository.GetByNameAsync(dto.Name, cancellationToken);
            if (existingRole != null)
            {
                return ApiResponse<RoleDto>.Error("نقش با این نام قبلاً وجود دارد", 400, "نام نقش باید یکتا باشد");
            }

            var role = _mapper.Map<Role>(dto);
            await _roleRepository.AddAsync(role, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role created with ID: {RoleId}", role.Id);
            var roleDto = _mapper.Map<RoleDto>(role);
            return ApiResponse<RoleDto>.Success(roleDto, "نقش جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return ApiResponse<RoleDto>.Error("خطا در ایجاد نقش", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی نقش
    /// </summary>
    public async Task<ApiResponse<RoleDto>> UpdateAsync(UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (role == null)
            {
                return ApiResponse<RoleDto>.Error($"نقش با شناسه {dto.Id} یافت نشد", 404);
            }

            // بررسی تکراری نبودن نام نقش
            if (dto.Name != role.Name)
            {
                var existingRole = await _roleRepository.GetByNameAsync(dto.Name, cancellationToken);
                if (existingRole != null && existingRole.Id != dto.Id)
                {
                    return ApiResponse<RoleDto>.Error("نقش با این نام قبلاً وجود دارد", 400, "نام نقش باید یکتا باشد");
                }
            }

            _mapper.Map(dto, role);
            await _roleRepository.UpdateAsync(role, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role updated with ID: {RoleId}", role.Id);
            var roleDto = _mapper.Map<RoleDto>(role);
            return ApiResponse<RoleDto>.Success(roleDto, "نقش با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role");
            return ApiResponse<RoleDto>.Error("خطا در به‌روزرسانی نقش", ex);
        }
    }

    /// <summary>
    /// حذف نقش
    /// </summary>
    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
            if (role == null)
            {
                return ApiResponse.Error("نقش با شناسه مشخص شده یافت نشد", 404);
            }

            await _roleRepository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role deleted with ID: {RoleId}", id);
            return ApiResponse.Success("نقش با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role with ID: {Id}", id);
            return ApiResponse.Error("خطا در حذف نقش", ex);
        }
    }
}
