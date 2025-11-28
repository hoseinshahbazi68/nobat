using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
/// سرویس تعرفه خدمت
/// این سرویس عملیات CRUD مربوط به تعرفه‌های خدمات را مدیریت می‌کند
/// شامل دریافت، ایجاد، به‌روزرسانی و حذف تعرفه‌ها
/// قیمت نهایی با در نظر گیری تخفیف‌ها محاسبه می‌شود
/// از Sieve برای فیلتر و مرتب‌سازی استفاده می‌کند
/// </summary>
public class ServiceTariffService : IServiceTariffService
{
    private readonly IRepository<ServiceTariff> _repository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<ServiceTariffService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceTariffService(
        IRepository<ServiceTariff> repository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<ServiceTariffService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ServiceTariffDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            var tariff = await query
                .Include(t => t.Service)
                .Include(t => t.Insurance)
                .Include(t => t.Clinic)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (tariff == null)
            {
                return ApiResponse<ServiceTariffDto>.Error("تعرفه خدمت با شناسه مشخص شده یافت نشد", 404);
            }

            var tariffDto = new ServiceTariffDto
            {
                Id = tariff.Id,
                ServiceId = tariff.ServiceId,
                ServiceName = tariff.Service?.Name,
                InsuranceId = tariff.InsuranceId,
                InsuranceName = tariff.Insurance?.Name,
                DoctorId = tariff.DoctorId,
                //DoctorName = tariff.Doctor != null ? $"{tariff.Doctor.FirstName} {tariff.Doctor.LastName}" : null,
                ClinicId = tariff.ClinicId,
                ClinicName = tariff.Clinic?.Name,
                Price = tariff.Price,
                FinalPrice = tariff.FinalPrice,
                VisitDuration = tariff.VisitDuration,
                CreatedAt = tariff.CreatedAt
            };

            return ApiResponse<ServiceTariffDto>.Success(tariffDto, "تعرفه خدمت با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service tariff by id: {Id}", id);
            return ApiResponse<ServiceTariffDto>.Error("خطا در دریافت تعرفه خدمت", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<ServiceTariffDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            query = query
                .Include(t => t.Service)
                .Include(t => t.Insurance)
                .Include(t => t.Clinic);

            var totalCount = await query.CountAsync(cancellationToken);
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var tariffs = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<ServiceTariffDto>
            {
                Items = tariffs.Select(t => new ServiceTariffDto
                {
                    Id = t.Id,
                    ServiceId = t.ServiceId,
                    ServiceName = t.Service?.Name,
                    InsuranceId = t.InsuranceId,
                    InsuranceName = t.Insurance?.Name,
                    DoctorId = t.DoctorId,
                    //DoctorName = t.Doctor != null ? $"{t.Doctor.FirstName} {t.Doctor.LastName}" : null,
                    ClinicId = t.ClinicId,
                    ClinicName = t.Clinic?.Name,
                    Price = t.Price,
                    FinalPrice = t.FinalPrice,
                    VisitDuration = t.VisitDuration,
                    CreatedAt = t.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<ServiceTariffDto>>.Success(result, "لیست تعرفه‌های خدمات با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all service tariffs");
            return ApiResponse<PagedResult<ServiceTariffDto>>.Error("خطا در دریافت لیست تعرفه‌های خدمات", ex);
        }
    }

    public async Task<ApiResponse<ServiceTariffDto>> CreateAsync(CreateServiceTariffDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariff = new ServiceTariff
            {
                ServiceId = dto.ServiceId,
                InsuranceId = dto.InsuranceId,
                DoctorId = dto.DoctorId,
                ClinicId = dto.ClinicId,
                Price = dto.Price,
                VisitDuration = dto.VisitDuration
            };

            await _repository.AddAsync(tariff, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ServiceTariff created with ID: {ServiceTariffId}", tariff.Id);

            var query = await _repository.GetQueryableAsync(cancellationToken);
            var createdTariff = await query
                .Include(t => t.Service)
                .Include(t => t.Insurance)
                .Include(t => t.Clinic)
                .FirstOrDefaultAsync(t => t.Id == tariff.Id, cancellationToken);

            var tariffDto = new ServiceTariffDto
            {
                Id = createdTariff!.Id,
                ServiceId = createdTariff.ServiceId,
                ServiceName = createdTariff.Service?.Name,
                InsuranceId = createdTariff.InsuranceId,
                InsuranceName = createdTariff.Insurance?.Name,
                DoctorId = createdTariff.DoctorId,
              //  DoctorName = createdTariff.Doctor != null ? $"{createdTariff.Doctor.FirstName} {createdTariff.Doctor.LastName}" : null,
                ClinicId = createdTariff.ClinicId,
                ClinicName = createdTariff.Clinic?.Name,
                Price = createdTariff.Price,
                FinalPrice = createdTariff.FinalPrice,
                VisitDuration = createdTariff.VisitDuration,
                CreatedAt = createdTariff.CreatedAt
            };

            return ApiResponse<ServiceTariffDto>.Success(tariffDto, "تعرفه خدمت جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service tariff");
            return ApiResponse<ServiceTariffDto>.Error("خطا در ایجاد تعرفه خدمت", ex);
        }
    }

    public async Task<ApiResponse<ServiceTariffDto>> UpdateAsync(UpdateServiceTariffDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var tariff = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (tariff == null)
            {
                return ApiResponse<ServiceTariffDto>.Error($"تعرفه خدمت با شناسه {dto.Id} یافت نشد", 404);
            }

            tariff.ServiceId = dto.ServiceId;
            tariff.InsuranceId = dto.InsuranceId;
            tariff.DoctorId = dto.DoctorId;
            tariff.ClinicId = dto.ClinicId;
            tariff.Price = dto.Price;
            tariff.VisitDuration = dto.VisitDuration;

            await _repository.UpdateAsync(tariff, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ServiceTariff updated with ID: {ServiceTariffId}", tariff.Id);

            var query = await _repository.GetQueryableAsync(cancellationToken);
            var updatedTariff = await query
                .Include(t => t.Service)
                .Include(t => t.Insurance)
                .Include(t => t.Clinic)
                .FirstOrDefaultAsync(t => t.Id == tariff.Id, cancellationToken);

            var tariffDto = new ServiceTariffDto
            {
                Id = updatedTariff!.Id,
                ServiceId = updatedTariff.ServiceId,
                ServiceName = updatedTariff.Service?.Name,
                InsuranceId = updatedTariff.InsuranceId,
                InsuranceName = updatedTariff.Insurance?.Name,
                DoctorId = updatedTariff.DoctorId,
                //DoctorName = updatedTariff.Doctor != null ? $"{updatedTariff.Doctor.FirstName} {updatedTariff.Doctor.LastName}" : null,
                ClinicId = updatedTariff.ClinicId,
                ClinicName = updatedTariff.Clinic?.Name,
                Price = updatedTariff.Price,
                FinalPrice = updatedTariff.FinalPrice,
                VisitDuration = updatedTariff.VisitDuration,
                CreatedAt = updatedTariff.CreatedAt
            };

            return ApiResponse<ServiceTariffDto>.Success(tariffDto, "تعرفه خدمت با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service tariff");
            return ApiResponse<ServiceTariffDto>.Error("خطا در به‌روزرسانی تعرفه خدمت", ex);
        }
    }

    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ServiceTariff deleted with ID: {ServiceTariffId}", id);
            return ApiResponse.Success("تعرفه خدمت با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service tariff");
            return ApiResponse.Error("خطا در حذف تعرفه خدمت", ex);
        }
    }
}
