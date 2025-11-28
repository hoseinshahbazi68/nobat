using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Doctors.Dto;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Doctors;

/// <summary>
/// سرویس اطلاعات ویزیت پزشک
/// </summary>
public class DoctorVisitInfoService : IDoctorVisitInfoService
{
    private readonly IRepository<DoctorVisitInfo> _repository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<DoctorVisitInfoService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DoctorVisitInfoService(
        IRepository<DoctorVisitInfo> repository,
        IRepository<Doctor> doctorRepository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<DoctorVisitInfoService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _doctorRepository = doctorRepository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<DoctorVisitInfoDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var visitInfo = await query
                .Include(v => v.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

            if (visitInfo == null)
            {
                return ApiResponse<DoctorVisitInfoDto>.Error("اطلاعات ویزیت یافت نشد", 404);
            }

            var dto = _mapper.Map<DoctorVisitInfoDto>(visitInfo);
            if (visitInfo.Doctor != null)
            {
                dto.DoctorName = $"{visitInfo.Doctor.User?.FirstName ?? ""} {visitInfo.Doctor.User?.LastName ?? ""}".Trim();
                dto.MedicalCode = visitInfo.Doctor.MedicalCode;
            }

            return ApiResponse<DoctorVisitInfoDto>.Success(dto, "اطلاعات ویزیت با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visit info by id: {Id}", id);
            return ApiResponse<DoctorVisitInfoDto>.Error("خطا در دریافت اطلاعات ویزیت", ex);
        }
    }

    public async Task<ApiResponse<DoctorVisitInfoDto>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var visitInfo = await query
                .Include(v => v.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(v => v.DoctorId == doctorId, cancellationToken);

            if (visitInfo == null)
            {
                return ApiResponse<DoctorVisitInfoDto>.Error("اطلاعات ویزیت برای این پزشک یافت نشد", 404);
            }

            var dto = _mapper.Map<DoctorVisitInfoDto>(visitInfo);
            if (visitInfo.Doctor != null)
            {
                dto.DoctorName = $"{visitInfo.Doctor.User?.FirstName ?? ""} {visitInfo.Doctor.User?.LastName ?? ""}".Trim();
                dto.MedicalCode = visitInfo.Doctor.MedicalCode;
            }

            return ApiResponse<DoctorVisitInfoDto>.Success(dto, "اطلاعات ویزیت با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visit info by doctor id: {DoctorId}", doctorId);
            return ApiResponse<DoctorVisitInfoDto>.Error("خطا در دریافت اطلاعات ویزیت", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<DoctorVisitInfoDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            query = query.Include(v => v.Doctor).ThenInclude(d => d.User);

            var totalCount = await query.CountAsync(cancellationToken);

            query = _sieveProcessor.Apply(sieveModel, query);

            var page = sieveModel.Page ?? 1;
            var pageSize = sieveModel.PageSize ?? 10;

            // در صورت عدم وجود OrderBy از Sieve، یک OrderBy پیش‌فرض اضافه می‌کنیم
            if (!query.Expression.ToString().Contains("OrderBy") && !query.Expression.ToString().Contains("OrderByDescending"))
            {
                query = query.OrderByDescending(v => v.Id);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var dtos = items.Select(v =>
            {
                var dto = _mapper.Map<DoctorVisitInfoDto>(v);
                if (v.Doctor != null)
                {
                    dto.DoctorName = $"{v.Doctor.User?.FirstName ?? ""} {v.Doctor.User?.LastName ?? ""}".Trim();
                    dto.MedicalCode = v.Doctor.MedicalCode;
                }
                return dto;
            }).ToList();

            var result = new PagedResult<DoctorVisitInfoDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return ApiResponse<PagedResult<DoctorVisitInfoDto>>.Success(result, "لیست اطلاعات ویزیت با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all visit infos");
            return ApiResponse<PagedResult<DoctorVisitInfoDto>>.Error("خطا در دریافت لیست اطلاعات ویزیت", ex);
        }
    }

    public async Task<ApiResponse<DoctorVisitInfoDto>> CreateAsync(CreateDoctorVisitInfoDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی وجود پزشک
            var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId, cancellationToken);
            if (doctor == null)
            {
                return ApiResponse<DoctorVisitInfoDto>.Error("پزشک یافت نشد", 404);
            }

            // بررسی اینکه آیا برای این پزشک قبلاً اطلاعات ویزیت ایجاد شده است
            var existingVisitInfo = await _repository.GetQueryableAsync(cancellationToken);
            var existing = await existingVisitInfo
                .FirstOrDefaultAsync(v => v.DoctorId == dto.DoctorId, cancellationToken);

            if (existing != null)
            {
                return ApiResponse<DoctorVisitInfoDto>.Error("برای این پزشک قبلاً اطلاعات ویزیت ایجاد شده است. لطفاً از به‌روزرسانی استفاده کنید", 400);
            }

            var visitInfo = _mapper.Map<DoctorVisitInfo>(dto);
            await _repository.AddAsync(visitInfo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resultDto = _mapper.Map<DoctorVisitInfoDto>(visitInfo);
            if (doctor.User != null)
            {
                resultDto.DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}".Trim();
                resultDto.MedicalCode = doctor.MedicalCode;
            }

            _logger.LogInformation("Visit info created with ID: {Id} for Doctor: {DoctorId}", visitInfo.Id, dto.DoctorId);
            return ApiResponse<DoctorVisitInfoDto>.Success(resultDto, "اطلاعات ویزیت با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating visit info");
            return ApiResponse<DoctorVisitInfoDto>.Error("خطا در ایجاد اطلاعات ویزیت", ex);
        }
    }

    public async Task<ApiResponse<DoctorVisitInfoDto>> UpdateAsync(UpdateDoctorVisitInfoDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var visitInfo = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (visitInfo == null)
            {
                return ApiResponse<DoctorVisitInfoDto>.Error("اطلاعات ویزیت یافت نشد", 404);
            }

            visitInfo.About = dto.About;
            visitInfo.ClinicAddress = dto.ClinicAddress;
            visitInfo.ClinicPhone = dto.ClinicPhone;
            visitInfo.OfficeHours = dto.OfficeHours;
            visitInfo.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(visitInfo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // بارگذاری مجدد با شامل کردن Doctor
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var updatedVisitInfo = await query
                .Include(v => v.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(v => v.Id == visitInfo.Id, cancellationToken);

            var resultDto = _mapper.Map<DoctorVisitInfoDto>(updatedVisitInfo!);
            if (updatedVisitInfo!.Doctor != null)
            {
                resultDto.DoctorName = $"{updatedVisitInfo.Doctor.User?.FirstName ?? ""} {updatedVisitInfo.Doctor.User?.LastName ?? ""}".Trim();
                resultDto.MedicalCode = updatedVisitInfo.Doctor.MedicalCode;
            }

            _logger.LogInformation("Visit info updated with ID: {Id}", dto.Id);
            return ApiResponse<DoctorVisitInfoDto>.Success(resultDto, "اطلاعات ویزیت با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating visit info");
            return ApiResponse<DoctorVisitInfoDto>.Error("خطا در به‌روزرسانی اطلاعات ویزیت", ex);
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var visitInfo = await _repository.GetByIdAsync(id, cancellationToken);
            if (visitInfo == null)
            {
                return ApiResponse<bool>.Error("اطلاعات ویزیت یافت نشد", 404);
            }

            await _repository.DeleteAsync(visitInfo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Visit info deleted with ID: {Id}", id);
            return ApiResponse<bool>.Success(true, "اطلاعات ویزیت با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting visit info");
            return ApiResponse<bool>.Error("خطا در حذف اطلاعات ویزیت", ex);
        }
    }
}
