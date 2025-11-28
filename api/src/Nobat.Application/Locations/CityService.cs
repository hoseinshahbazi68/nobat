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
/// سرویس شهر
/// </summary>
public class CityService : ICityService
{
    private readonly IRepository<City> _repository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<CityService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CityService(
        IRepository<City> repository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<CityService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<CityDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var city = await query
                .Include(c => c.Province)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (city == null)
            {
                return ApiResponse<CityDto>.Error("شهر با شناسه مشخص شده یافت نشد", 404);
            }

            var cityDto = new CityDto
            {
                Id = city.Id,
                Name = city.Name,
                ProvinceId = city.ProvinceId,
                ProvinceName = city.Province?.Name,
                Code = city.Code,
                CreatedAt = city.CreatedAt
            };

            return ApiResponse<CityDto>.Success(cityDto, "شهر با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting city by id: {Id}", id);
            return ApiResponse<CityDto>.Error("خطا در دریافت شهر", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<CityDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            query = query.Include(c => c.Province);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var cities = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<CityDto>
            {
                Items = cities.Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProvinceId = c.ProvinceId,
                    ProvinceName = c.Province?.Name,
                    Code = c.Code,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<CityDto>>.Success(result, "لیست شهرها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all cities");
            return ApiResponse<PagedResult<CityDto>>.Error("خطا در دریافت لیست شهرها", ex);
        }
    }

    public async Task<ApiResponse<CityDto>> CreateAsync(CreateCityDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var city = _mapper.Map<City>(dto);
            await _repository.AddAsync(city, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("City created with ID: {CityId}", city.Id);

            var query = await _repository.GetQueryableAsync(cancellationToken);
            var createdCity = await query
                .Include(c => c.Province)
                .FirstOrDefaultAsync(c => c.Id == city.Id, cancellationToken);

            var cityDto = new CityDto
            {
                Id = createdCity!.Id,
                Name = createdCity.Name,
                ProvinceId = createdCity.ProvinceId,
                ProvinceName = createdCity.Province?.Name,
                Code = createdCity.Code,
                CreatedAt = createdCity.CreatedAt
            };

            return ApiResponse<CityDto>.Success(cityDto, "شهر جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating city");
            return ApiResponse<CityDto>.Error("خطا در ایجاد شهر", ex);
        }
    }

    public async Task<ApiResponse<CityDto>> UpdateAsync(UpdateCityDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var city = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (city == null)
            {
                return ApiResponse<CityDto>.Error($"شهر با شناسه {dto.Id} یافت نشد", 404);
            }

            _mapper.Map(dto, city);
            await _repository.UpdateAsync(city, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("City updated with ID: {CityId}", city.Id);

            var query = await _repository.GetQueryableAsync(cancellationToken);
            var updatedCity = await query
                .Include(c => c.Province)
                .FirstOrDefaultAsync(c => c.Id == city.Id, cancellationToken);

            var cityDto = new CityDto
            {
                Id = updatedCity!.Id,
                Name = updatedCity.Name,
                ProvinceId = updatedCity.ProvinceId,
                ProvinceName = updatedCity.Province?.Name,
                Code = updatedCity.Code,
                CreatedAt = updatedCity.CreatedAt
            };

            return ApiResponse<CityDto>.Success(cityDto, "شهر با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating city");
            return ApiResponse<CityDto>.Error("خطا در به‌روزرسانی شهر", ex);
        }
    }

    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("City deleted with ID: {CityId}", id);
            return ApiResponse.Success("شهر با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting city");
            return ApiResponse.Error("خطا در حذف شهر", ex);
        }
    }

    public async Task<ApiResponse<List<CityDto>>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var cities = await query
                .Include(c => c.Province)
                .Where(c => c.ProvinceId == provinceId)
                .ToListAsync(cancellationToken);

            var cityDtos = cities.Select(c => new CityDto
            {
                Id = c.Id,
                Name = c.Name,
                ProvinceId = c.ProvinceId,
                ProvinceName = c.Province?.Name,
                Code = c.Code,
                CreatedAt = c.CreatedAt
            }).ToList();

            return ApiResponse<List<CityDto>>.Success(cityDtos, "لیست شهرهای استان با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cities by province id: {ProvinceId}", provinceId);
            return ApiResponse<List<CityDto>>.Error("خطا در دریافت لیست شهرهای استان", ex);
        }
    }
}
