using AutoMapper;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Services.Dto;
using Nobat.Domain.Entities.Services;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Services;

/// <summary>
/// سرویس خدمت
/// این سرویس عملیات CRUD مربوط به خدمات را مدیریت می‌کند
/// شامل دریافت، ایجاد، به‌روزرسانی و حذف خدمات
/// از Sieve برای فیلتر و مرتب‌سازی استفاده می‌کند
/// </summary>
public class ServiceService : IServiceService
{
    private readonly IRepository<Service> _repository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<ServiceService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceService(
        IRepository<Service> repository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<ServiceService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ServiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = await _repository.GetByIdAsync(id, cancellationToken);
            if (service == null)
            {
                return ApiResponse<ServiceDto>.Error("خدمت با شناسه مشخص شده یافت نشد", 404);
            }

            var serviceDto = _mapper.Map<ServiceDto>(service);
            return ApiResponse<ServiceDto>.Success(serviceDto, "خدمت با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service by id: {Id}", id);
            return ApiResponse<ServiceDto>.Error("خطا در دریافت خدمت", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<ServiceDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var services = filteredQuery.ToList();

            var result = new PagedResult<ServiceDto>
            {
                Items = _mapper.Map<List<ServiceDto>>(services),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<ServiceDto>>.Success(result, "لیست خدمات با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all services");
            return ApiResponse<PagedResult<ServiceDto>>.Error("خطا در دریافت لیست خدمات", ex);
        }
    }

    public async Task<ApiResponse<ServiceDto>> CreateAsync(CreateServiceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = _mapper.Map<Service>(dto);
            await _repository.AddAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service created with ID: {ServiceId}", service.Id);
            var serviceDto = _mapper.Map<ServiceDto>(service);
            return ApiResponse<ServiceDto>.Success(serviceDto, "خدمت جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service");
            return ApiResponse<ServiceDto>.Error("خطا در ایجاد خدمت", ex);
        }
    }

    public async Task<ApiResponse<ServiceDto>> UpdateAsync(UpdateServiceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (service == null)
            {
                return ApiResponse<ServiceDto>.Error($"خدمت با شناسه {dto.Id} یافت نشد", 404);
            }

            _mapper.Map(dto, service);
            await _repository.UpdateAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service updated with ID: {ServiceId}", service.Id);
            var serviceDto = _mapper.Map<ServiceDto>(service);
            return ApiResponse<ServiceDto>.Success(serviceDto, "خدمت با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service");
            return ApiResponse<ServiceDto>.Error("خطا در به‌روزرسانی خدمت", ex);
        }
    }

    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service deleted with ID: {ServiceId}", id);
            return ApiResponse.Success("خدمت با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service");
            return ApiResponse.Error("خطا در حذف خدمت", ex);
        }
    }
}
