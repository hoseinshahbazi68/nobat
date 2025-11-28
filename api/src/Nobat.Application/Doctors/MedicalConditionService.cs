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
/// سرویس مدیریت علائم پزشکی
/// </summary>
public class MedicalConditionService : IMedicalConditionService
{
    private readonly IRepository<MedicalCondition> _medicalConditionRepository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<MedicalConditionService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// سازنده سرویس علائم پزشکی
    /// </summary>
    public MedicalConditionService(
        IRepository<MedicalCondition> medicalConditionRepository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<MedicalConditionService> logger,
        IUnitOfWork unitOfWork)
    {
        _medicalConditionRepository = medicalConditionRepository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// دریافت علائم بر اساس شناسه
    /// </summary>
    public async Task<ApiResponse<MedicalConditionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var medicalCondition = await _medicalConditionRepository.GetByIdAsync(id, cancellationToken);
            if (medicalCondition == null)
            {
                return ApiResponse<MedicalConditionDto>.Error("علائم با شناسه مشخص شده یافت نشد", 404);
            }

            var medicalConditionDto = _mapper.Map<MedicalConditionDto>(medicalCondition);
            return ApiResponse<MedicalConditionDto>.Success(medicalConditionDto, "علائم با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical condition by id: {Id}", id);
            return ApiResponse<MedicalConditionDto>.Error("خطا در دریافت علائم", ex);
        }
    }

    /// <summary>
    /// دریافت لیست علائم با فیلتر و مرتب‌سازی
    /// </summary>
    public async Task<ApiResponse<PagedResult<MedicalConditionDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _medicalConditionRepository.GetQueryableNoTrackingAsync(cancellationToken);

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var medicalConditions = filteredQuery.ToList();

            var result = new PagedResult<MedicalConditionDto>
            {
                Items = _mapper.Map<List<MedicalConditionDto>>(medicalConditions),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<MedicalConditionDto>>.Success(result, "لیست علائم با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all medical conditions");
            return ApiResponse<PagedResult<MedicalConditionDto>>.Error("خطا در دریافت لیست علائم", ex);
        }
    }

    /// <summary>
    /// ایجاد علائم جدید
    /// </summary>
    public async Task<ApiResponse<MedicalConditionDto>> CreateAsync(CreateMedicalConditionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی تکراری نبودن نام علائم
            var existingConditions = await _medicalConditionRepository.FindAsync(mc => mc.Name == dto.Name, cancellationToken);
            if (existingConditions.Any())
            {
                return ApiResponse<MedicalConditionDto>.Error("علائم با این نام قبلاً وجود دارد", 400, "نام علائم باید یکتا باشد");
            }

            var medicalCondition = _mapper.Map<MedicalCondition>(dto);
            await _medicalConditionRepository.AddAsync(medicalCondition, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Medical condition created with ID: {MedicalConditionId}", medicalCondition.Id);
            var medicalConditionDto = _mapper.Map<MedicalConditionDto>(medicalCondition);
            return ApiResponse<MedicalConditionDto>.Success(medicalConditionDto, "علائم جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medical condition");
            return ApiResponse<MedicalConditionDto>.Error("خطا در ایجاد علائم", ex);
        }
    }

    /// <summary>
    /// به‌روزرسانی علائم
    /// </summary>
    public async Task<ApiResponse<MedicalConditionDto>> UpdateAsync(UpdateMedicalConditionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var medicalCondition = await _medicalConditionRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (medicalCondition == null)
            {
                return ApiResponse<MedicalConditionDto>.Error($"علائم با شناسه {dto.Id} یافت نشد", 404);
            }

            // بررسی تکراری نبودن نام علائم
            if (dto.Name != medicalCondition.Name)
            {
                var existingConditions = await _medicalConditionRepository.FindAsync(mc => mc.Name == dto.Name, cancellationToken);
                if (existingConditions.Any() && existingConditions.First().Id != dto.Id)
                {
                    return ApiResponse<MedicalConditionDto>.Error("علائم با این نام قبلاً وجود دارد", 400, "نام علائم باید یکتا باشد");
                }
            }

            _mapper.Map(dto, medicalCondition);
            await _medicalConditionRepository.UpdateAsync(medicalCondition, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Medical condition updated with ID: {MedicalConditionId}", medicalCondition.Id);
            var medicalConditionDto = _mapper.Map<MedicalConditionDto>(medicalCondition);
            return ApiResponse<MedicalConditionDto>.Success(medicalConditionDto, "علائم با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medical condition");
            return ApiResponse<MedicalConditionDto>.Error("خطا در به‌روزرسانی علائم", ex);
        }
    }

    /// <summary>
    /// حذف علائم
    /// </summary>
    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var medicalCondition = await _medicalConditionRepository.GetByIdAsync(id, cancellationToken);
            if (medicalCondition == null)
            {
                return ApiResponse.Error("علائم با شناسه مشخص شده یافت نشد", 404);
            }

            await _medicalConditionRepository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Medical condition deleted with ID: {MedicalConditionId}", id);
            return ApiResponse.Success("علائم با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting medical condition with ID: {Id}", id);
            return ApiResponse.Error("خطا در حذف علائم", ex);
        }
    }

    /// <summary>
    /// جستجوی علائم بر اساس نام
    /// </summary>
    public async Task<ApiResponse<List<MedicalConditionDto>>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return ApiResponse<List<MedicalConditionDto>>.Error("متن جستجو نمی‌تواند خالی باشد", 400);
            }

            var dbQuery = await _medicalConditionRepository.GetQueryableAsync(cancellationToken);
            var queryLower = query.ToLower();

            var medicalConditions = await dbQuery
                .Where(mc => mc.Name.ToLower().Contains(queryLower) ||
                           (mc.Description != null && mc.Description.ToLower().Contains(queryLower)))
                .OrderBy(mc => mc.Name)
                .Take(20)
                .ToListAsync(cancellationToken);

            var medicalConditionDtos = _mapper.Map<List<MedicalConditionDto>>(medicalConditions);
            return ApiResponse<List<MedicalConditionDto>>.Success(medicalConditionDtos, "جستجو با موفقیت انجام شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching medical conditions");
            return ApiResponse<List<MedicalConditionDto>>.Error("خطا در جستجوی علائم", ex);
        }
    }
}
