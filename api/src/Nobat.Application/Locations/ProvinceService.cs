using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Locations.Dto;
using Nobat.Domain.Entities.Locations;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Locations;

/// <summary>
/// سرویس استان
/// </summary>
public class ProvinceService : IProvinceService
{
    private readonly IRepository<Province> _repository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<ProvinceService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProvinceService(
        IRepository<Province> repository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<ProvinceService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ProvinceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var province = await _repository.GetByIdAsync(id, cancellationToken);
            if (province == null)
            {
                return ApiResponse<ProvinceDto>.Error("استان با شناسه مشخص شده یافت نشد", 404);
            }

            var provinceDto = _mapper.Map<ProvinceDto>(province);
            return ApiResponse<ProvinceDto>.Success(provinceDto, "استان با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting province by id: {Id}", id);
            return ApiResponse<ProvinceDto>.Error("خطا در دریافت استان", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<ProvinceDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var provinces = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<ProvinceDto>
            {
                Items = _mapper.Map<List<ProvinceDto>>(provinces),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<ProvinceDto>>.Success(result, "لیست استان‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all provinces");
            return ApiResponse<PagedResult<ProvinceDto>>.Error("خطا در دریافت لیست استان‌ها", ex);
        }
    }

    public async Task<ApiResponse<ProvinceDto>> CreateAsync(CreateProvinceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var province = _mapper.Map<Province>(dto);
            await _repository.AddAsync(province, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Province created with ID: {ProvinceId}", province.Id);
            var provinceDto = _mapper.Map<ProvinceDto>(province);
            return ApiResponse<ProvinceDto>.Success(provinceDto, "استان جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating province");
            return ApiResponse<ProvinceDto>.Error("خطا در ایجاد استان", ex);
        }
    }

    public async Task<ApiResponse<ProvinceDto>> UpdateAsync(UpdateProvinceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var province = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (province == null)
            {
                return ApiResponse<ProvinceDto>.Error($"استان با شناسه {dto.Id} یافت نشد", 404);
            }

            _mapper.Map(dto, province);
            await _repository.UpdateAsync(province, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Province updated with ID: {ProvinceId}", province.Id);
            var provinceDto = _mapper.Map<ProvinceDto>(province);
            return ApiResponse<ProvinceDto>.Success(provinceDto, "استان با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating province");
            return ApiResponse<ProvinceDto>.Error("خطا در به‌روزرسانی استان", ex);
        }
    }

    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Province deleted with ID: {ProvinceId}", id);
            return ApiResponse.Success("استان با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting province");
            return ApiResponse.Error("خطا در حذف استان", ex);
        }
    }
}
