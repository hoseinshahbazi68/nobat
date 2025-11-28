using AutoMapper;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Insurances.Dto;
using Nobat.Application.Schedules;
using Nobat.Domain.Entities.Insurances;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Insurances;

/// <summary>
/// سرویس بیمه
/// این سرویس عملیات CRUD مربوط به بیمه‌ها را مدیریت می‌کند
/// شامل دریافت، ایجاد، به‌روزرسانی و حذف بیمه‌ها
/// از Sieve برای فیلتر و مرتب‌سازی استفاده می‌کند
/// </summary>
public class InsuranceService : IInsuranceService
{
    private readonly IRepository<Insurance> _repository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<InsuranceService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public InsuranceService(
        IRepository<Insurance> repository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<InsuranceService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<InsuranceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var insurance = await _repository.GetByIdAsync(id, cancellationToken);
            if (insurance == null)
            {
                return ApiResponse<InsuranceDto>.Error("بیمه با شناسه مشخص شده یافت نشد", 404);
            }

            var insuranceDto = _mapper.Map<InsuranceDto>(insurance);
            return ApiResponse<InsuranceDto>.Success(insuranceDto, "بیمه با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting insurance by id: {Id}", id);
            return ApiResponse<InsuranceDto>.Error("خطا در دریافت بیمه", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<InsuranceDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var insurances = filteredQuery.ToList();

            var result = new PagedResult<InsuranceDto>
            {
                Items = _mapper.Map<List<InsuranceDto>>(insurances),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<InsuranceDto>>.Success(result, "لیست بیمه‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all insurances");
            return ApiResponse<PagedResult<InsuranceDto>>.Error("خطا در دریافت لیست بیمه‌ها", ex);
        }
    }

    public async Task<ApiResponse<InsuranceDto>> CreateAsync(CreateInsuranceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var insurance = _mapper.Map<Insurance>(dto);
            await _repository.AddAsync(insurance, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Insurance created with ID: {InsuranceId}", insurance.Id);
            var insuranceDto = _mapper.Map<InsuranceDto>(insurance);
            return ApiResponse<InsuranceDto>.Success(insuranceDto, "بیمه جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating insurance");
            return ApiResponse<InsuranceDto>.Error("خطا در ایجاد بیمه", ex);
        }
    }

    public async Task<ApiResponse<InsuranceDto>> UpdateAsync(UpdateInsuranceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var insurance = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (insurance == null)
            {
                return ApiResponse<InsuranceDto>.Error($"بیمه با شناسه {dto.Id} یافت نشد", 404);
            }

            _mapper.Map(dto, insurance);
            await _repository.UpdateAsync(insurance, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Insurance updated with ID: {InsuranceId}", insurance.Id);
            var insuranceDto = _mapper.Map<InsuranceDto>(insurance);
            return ApiResponse<InsuranceDto>.Success(insuranceDto, "بیمه با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating insurance");
            return ApiResponse<InsuranceDto>.Error("خطا در به‌روزرسانی بیمه", ex);
        }
    }

    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Insurance deleted with ID: {InsuranceId}", id);
            return ApiResponse.Success("بیمه با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting insurance");
            return ApiResponse.Error("خطا در حذف بیمه", ex);
        }
    }
}
