using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Repositories;
using Nobat.Application.Doctors.Dto;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Doctors;

/// <summary>
/// سرویس مدیریت تخصص‌ها
/// </summary>
public class SpecialtyService : ISpecialtyService
{
    private readonly IRepository<Specialty> _specialtyRepository;
    private readonly IRepository<SpecialtyMedicalCondition> _specialtyMedicalConditionRepository;
    private readonly IRepository<MedicalCondition> _medicalConditionRepository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<SpecialtyService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// سازنده سرویس تخصص
    /// </summary>
    public SpecialtyService(
        IRepository<Specialty> specialtyRepository,
        IRepository<SpecialtyMedicalCondition> specialtyMedicalConditionRepository,
        IRepository<MedicalCondition> medicalConditionRepository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<SpecialtyService> logger,
        IUnitOfWork unitOfWork)
    {
        _specialtyRepository = specialtyRepository;
        _specialtyMedicalConditionRepository = specialtyMedicalConditionRepository;
        _medicalConditionRepository = medicalConditionRepository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// دریافت تخصص بر اساس شناسه
    /// </summary>
    public async Task<ApiResponse<SpecialtyDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id, cancellationToken);
            if (specialty == null)
            {
                return ApiResponse<SpecialtyDto>.Error("تخصص با شناسه مشخص شده یافت نشد", 404);
            }

            var specialtyDto = _mapper.Map<SpecialtyDto>(specialty);
            return ApiResponse<SpecialtyDto>.Success(specialtyDto, "تخصص با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting specialty by id: {Id}", id);
            return ApiResponse<SpecialtyDto>.Error("خطا در دریافت تخصص", ex);
        }
    }

    /// <summary>
    /// دریافت لیست تخصص‌ها با فیلتر و مرتب‌سازی
    /// </summary>
    public async Task<ApiResponse<PagedResult<SpecialtyDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _specialtyRepository.GetQueryableNoTrackingAsync(cancellationToken);

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var specialties = filteredQuery.ToList();

            var result = new PagedResult<SpecialtyDto>
            {
                Items = _mapper.Map<List<SpecialtyDto>>(specialties),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<SpecialtyDto>>.Success(result, "لیست تخصص‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all specialties");
            return ApiResponse<PagedResult<SpecialtyDto>>.Error("خطا در دریافت لیست تخصص‌ها", ex);
        }
    }

    /// <summary>
    /// ایجاد تخصص جدید
    /// </summary>
    public async Task<ApiResponse<SpecialtyDto>> CreateAsync(CreateSpecialtyDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی تکراری نبودن نام تخصص
            var existingSpecialties = await _specialtyRepository.FindAsync(s => s.Name == dto.Name, cancellationToken);
            if (existingSpecialties.Any())
            {
                return ApiResponse<SpecialtyDto>.Error("تخصص با این نام قبلاً وجود دارد", 400, "نام تخصص باید یکتا باشد");
            }

            var specialty = _mapper.Map<Specialty>(dto);
            await _specialtyRepository.AddAsync(specialty, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Specialty created with ID: {SpecialtyId}", specialty.Id);
            var specialtyDto = _mapper.Map<SpecialtyDto>(specialty);
            return ApiResponse<SpecialtyDto>.Success(specialtyDto, "تخصص جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating specialty");
            return ApiResponse<SpecialtyDto>.Error("خطا در ایجاد تخصص", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی تخصص
    /// </summary>
    public async Task<ApiResponse<SpecialtyDto>> UpdateAsync(UpdateSpecialtyDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var specialty = await _specialtyRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (specialty == null)
            {
                return ApiResponse<SpecialtyDto>.Error($"تخصص با شناسه {dto.Id} یافت نشد", 404);
            }

            // بررسی تکراری نبودن نام تخصص
            if (dto.Name != specialty.Name)
            {
                var existingSpecialties = await _specialtyRepository.FindAsync(s => s.Name == dto.Name, cancellationToken);
                if (existingSpecialties.Any() && existingSpecialties.First().Id != dto.Id)
                {
                    return ApiResponse<SpecialtyDto>.Error("تخصص با این نام قبلاً وجود دارد", 400, "نام تخصص باید یکتا باشد");
                }
            }

            _mapper.Map(dto, specialty);
            await _specialtyRepository.UpdateAsync(specialty, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Specialty updated with ID: {SpecialtyId}", specialty.Id);
            var specialtyDto = _mapper.Map<SpecialtyDto>(specialty);
            return ApiResponse<SpecialtyDto>.Success(specialtyDto, "تخصص با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating specialty");
            return ApiResponse<SpecialtyDto>.Error("خطا در به‌روزرسانی تخصص", ex);
        }
    }

    /// <summary>
    /// حذف تخصص
    /// </summary>
    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id, cancellationToken);
            if (specialty == null)
            {
                return ApiResponse.Error("تخصص با شناسه مشخص شده یافت نشد", 404);
            }

            await _specialtyRepository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Specialty deleted with ID: {SpecialtyId}", id);
            return ApiResponse.Success("تخصص با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting specialty with ID: {Id}", id);
            return ApiResponse.Error("خطا در حذف تخصص", ex);
        }
    }

    /// <summary>
    /// دریافت لیست علائم پزشکی مرتبط با تخصص
    /// </summary>
    public async Task<ApiResponse<List<SpecialtyMedicalConditionDto>>> GetMedicalConditionsAsync(int specialtyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var specialty = await _specialtyRepository.GetByIdAsync(specialtyId, cancellationToken);
            if (specialty == null)
            {
                return ApiResponse<List<SpecialtyMedicalConditionDto>>.Error("تخصص با شناسه مشخص شده یافت نشد", 404);
            }

            var query = await _specialtyMedicalConditionRepository.GetQueryableAsync(cancellationToken);
            var specialtyMedicalConditions = await query
                .Include(smc => smc.MedicalCondition)
                .Where(smc => smc.SpecialtyId == specialtyId)
                .ToListAsync(cancellationToken);

            var result = _mapper.Map<List<SpecialtyMedicalConditionDto>>(specialtyMedicalConditions);
            return ApiResponse<List<SpecialtyMedicalConditionDto>>.Success(result, "لیست علائم پزشکی با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical conditions for specialty: {SpecialtyId}", specialtyId);
            return ApiResponse<List<SpecialtyMedicalConditionDto>>.Error("خطا در دریافت لیست علائم پزشکی", ex);
        }
    }

    /// <summary>
    /// افزودن علائم پزشکی به تخصص
    /// </summary>
    public async Task<ApiResponse<SpecialtyMedicalConditionDto>> AddMedicalConditionAsync(int specialtyId, int medicalConditionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var specialty = await _specialtyRepository.GetByIdAsync(specialtyId, cancellationToken);
            if (specialty == null)
            {
                return ApiResponse<SpecialtyMedicalConditionDto>.Error("تخصص با شناسه مشخص شده یافت نشد", 404);
            }

            var medicalCondition = await _medicalConditionRepository.GetByIdAsync(medicalConditionId, cancellationToken);
            if (medicalCondition == null)
            {
                return ApiResponse<SpecialtyMedicalConditionDto>.Error("علائم پزشکی با شناسه مشخص شده یافت نشد", 404);
            }

            // بررسی وجود قبلی رابطه
            var query = await _specialtyMedicalConditionRepository.GetQueryableAsync(cancellationToken);
            var existing = await query
                .FirstOrDefaultAsync(smc => smc.SpecialtyId == specialtyId && smc.MedicalConditionId == medicalConditionId, cancellationToken);

            if (existing != null)
            {
                return ApiResponse<SpecialtyMedicalConditionDto>.Error("این علائم پزشکی قبلاً به این تخصص اضافه شده است", 400);
            }

            var specialtyMedicalCondition = new SpecialtyMedicalCondition
            {
                SpecialtyId = specialtyId,
                MedicalConditionId = medicalConditionId
            };

            await _specialtyMedicalConditionRepository.AddAsync(specialtyMedicalCondition, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // بارگذاری مجدد با اطلاعات کامل
            var reloadedQuery = await _specialtyMedicalConditionRepository.GetQueryableAsync(cancellationToken);
            var reloaded = await reloadedQuery
                .Include(smc => smc.MedicalCondition)
                .FirstOrDefaultAsync(smc => smc.Id == specialtyMedicalCondition.Id, cancellationToken);

            var result = _mapper.Map<SpecialtyMedicalConditionDto>(reloaded);
            _logger.LogInformation("Medical condition {MedicalConditionId} added to specialty {SpecialtyId}", medicalConditionId, specialtyId);
            return ApiResponse<SpecialtyMedicalConditionDto>.Success(result, "علائم پزشکی با موفقیت اضافه شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding medical condition to specialty");
            return ApiResponse<SpecialtyMedicalConditionDto>.Error("خطا در افزودن علائم پزشکی", ex);
        }
    }

    /// <summary>
    /// حذف علائم پزشکی از تخصص
    /// </summary>
    public async Task<ApiResponse> RemoveMedicalConditionAsync(int specialtyId, int medicalConditionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _specialtyMedicalConditionRepository.GetQueryableAsync(cancellationToken);
            var specialtyMedicalCondition = await query
                .FirstOrDefaultAsync(smc => smc.SpecialtyId == specialtyId && smc.MedicalConditionId == medicalConditionId, cancellationToken);

            if (specialtyMedicalCondition == null)
            {
                return ApiResponse.Error("رابطه بین تخصص و علائم پزشکی یافت نشد", 404);
            }

            await _specialtyMedicalConditionRepository.DeleteAsync(specialtyMedicalCondition.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Medical condition {MedicalConditionId} removed from specialty {SpecialtyId}", medicalConditionId, specialtyId);
            return ApiResponse.Success("علائم پزشکی با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing medical condition from specialty");
            return ApiResponse.Error("خطا در حذف علائم پزشکی", ex);
        }
    }
}
