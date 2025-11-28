using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Common;
using Nobat.Application.Doctors.Dto;
using Nobat.Application.Schedules;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Entities.Services;
using Nobat.Domain.Interfaces;
using Nobat.Domain.Enums;
using Nobat.Domain.Extensions;
using Sieve.Models;
using Sieve.Services;

namespace Nobat.Application.Doctors;

/// <summary>
/// سرویس برنامه پزشک
/// این سرویس عملیات CRUD مربوط به برنامه‌های پزشکان را مدیریت می‌کند
/// شامل دریافت، ایجاد، به‌روزرسانی و حذف برنامه‌ها
/// همچنین امکان دریافت برنامه هفتگی و تولید خودکار برنامه برای بازه زمانی مشخص
/// از Sieve برای فیلتر و مرتب‌سازی استفاده می‌کند
/// </summary>
public class DoctorScheduleService : IDoctorScheduleService
{
    private readonly IRepository<DoctorSchedule> _repository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<ServiceTariff> _serviceTariffRepository;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ILogger<DoctorScheduleService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DoctorScheduleService(
        IRepository<DoctorSchedule> repository,
        IRepository<Doctor> doctorRepository,
        IRepository<Shift> shiftRepository,
        IRepository<Service> serviceRepository,
        IRepository<ServiceTariff> serviceTariffRepository,
        IMapper mapper,
        ISieveProcessor sieveProcessor,
        ILogger<DoctorScheduleService> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _doctorRepository = doctorRepository;
        _shiftRepository = shiftRepository;
        _serviceRepository = serviceRepository;
        _serviceTariffRepository = serviceTariffRepository;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<DoctorScheduleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            var schedule = await query
                .Include(s => s.Doctor)
                .Include(s => s.Shift)
                .Include(s => s.Clinic)
                .Include(s => s.Service)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (schedule == null)
            {
                return ApiResponse<DoctorScheduleDto>.Error("برنامه پزشک با شناسه مشخص شده یافت نشد", 404);
            }

            var scheduleDto = MapToDto(schedule);
            return ApiResponse<DoctorScheduleDto>.Success(scheduleDto, "برنامه پزشک با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor schedule by id: {Id}", id);
            return ApiResponse<DoctorScheduleDto>.Error("خطا در دریافت برنامه پزشک", ex);
        }
    }

    public async Task<ApiResponse<PagedResult<DoctorScheduleDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableNoTrackingAsync(cancellationToken);
            query = query
                .Include(s => s.Doctor)
                .Include(s => s.Shift)
                .Include(s => s.Clinic)
                .Include(s => s.Service);

            var totalCount = query.Count();
            var filteredQuery = _sieveProcessor.Apply(sieveModel, query);
            var schedules = await filteredQuery.ToListAsync(cancellationToken);

            var result = new PagedResult<DoctorScheduleDto>
            {
                Items = schedules.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = sieveModel.Page ?? 1,
                PageSize = sieveModel.PageSize ?? 10
            };

            return ApiResponse<PagedResult<DoctorScheduleDto>>.Success(result, "لیست برنامه‌های پزشک با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all doctor schedules");
            return ApiResponse<PagedResult<DoctorScheduleDto>>.Error("خطا در دریافت لیست برنامه‌های پزشک", ex);
        }
    }

    public async Task<ApiResponse<WeeklyScheduleDto>> GetWeeklyScheduleAsync(int doctorId, string weekStartDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _repository.GetQueryableAsync(cancellationToken);
            var schedules = await query
                .Include(s => s.Doctor)
                .Include(s => s.Shift)
                .Include(s => s.Clinic)
                .Include(s => s.Service)
                .Where(s => s.DoctorId == doctorId)
                .ToListAsync(cancellationToken);

            var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);

            var weeklySchedule = new WeeklyScheduleDto
            {
                DoctorId = doctorId,
               // DoctorName = doctor != null ? $"{doctor.FirstName} {doctor.LastName}" : null,
                Schedules = schedules.Select(MapToDto).ToList()
            };

            return ApiResponse<WeeklyScheduleDto>.Success(weeklySchedule, "برنامه هفتگی پزشک با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weekly schedule for doctor: {DoctorId}", doctorId);
            return ApiResponse<WeeklyScheduleDto>.Error("خطا در دریافت برنامه هفتگی پزشک", ex);
        }
    }

    public async Task<ApiResponse<DoctorScheduleDto>> CreateAsync(CreateDoctorScheduleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if(dto.ClinicId==0)
                return ApiResponse<DoctorScheduleDto>.Error("کلینک انتخاب نشده است", 400);

            // بررسی وجود تعرفه برای پزشک، کلینیک و خدمت
            var hasTariff = await CheckTariffExistsAsync(dto.DoctorId, dto.ClinicId, dto.ServiceId, cancellationToken);
            if (!hasTariff)
            {
                var errorMessage = $"برای پزشک، کلینیک و خدمت مورد نظر تعرفه‌ای تعریف نشده است. لطفاً ابتدا تعرفه را تعریف کنید.";
                return ApiResponse<DoctorScheduleDto>.Error(errorMessage, 400);
            }

            var schedule = new DoctorSchedule
            {
                DoctorId = dto.DoctorId,
                ShiftId = dto.ShiftId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = TimeSpan.Parse(dto.StartTime),
                EndTime = TimeSpan.Parse(dto.EndTime),
                Count = dto.Count,
                ClinicId = dto.ClinicId,
                ServiceId = dto.ServiceId
            };

            await _repository.AddAsync(schedule, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("DoctorSchedule created with ID: {DoctorScheduleId}", schedule.Id);

            var query = await _repository.GetQueryableAsync(cancellationToken);
            var createdSchedule = await query
                .Include(s => s.Doctor)
                .Include(s => s.Shift)
                .Include(s => s.Clinic)
                .FirstOrDefaultAsync(s => s.Id == schedule.Id, cancellationToken);

            var scheduleDto = MapToDto(createdSchedule!);
            return ApiResponse<DoctorScheduleDto>.Success(scheduleDto, "برنامه پزشک جدید با موفقیت ایجاد شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor schedule");
            return ApiResponse<DoctorScheduleDto>.Error("خطا در ایجاد برنامه پزشک", ex);
        }
    }

    public async Task<ApiResponse<DoctorScheduleDto>> UpdateAsync(UpdateDoctorScheduleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var schedule = await _repository.GetByIdAsync(dto.Id, cancellationToken);
            if (schedule == null)
            {
                return ApiResponse<DoctorScheduleDto>.Error($"برنامه پزشک با شناسه {dto.Id} یافت نشد", 404);
            }

            // بررسی وجود تعرفه برای پزشک، کلینیک و خدمت جدید (در صورت تغییر)
            // اگر پزشک، کلینیک یا خدمت تغییر کرده باشد، باید تعرفه جدید بررسی شود
            if (schedule.DoctorId != dto.DoctorId || schedule.ClinicId != dto.ClinicId || schedule.ServiceId != dto.ServiceId)
            {
                var hasTariff = await CheckTariffExistsAsync(dto.DoctorId, dto.ClinicId, dto.ServiceId, cancellationToken);
                if (!hasTariff)
                {
                    var errorMessage = dto.ClinicId.HasValue
                        ? $"برای پزشک، کلینیک و خدمت مورد نظر تعرفه‌ای تعریف نشده است. لطفاً ابتدا تعرفه را تعریف کنید."
                        : $"برای پزشک و خدمت مورد نظر تعرفه‌ای تعریف نشده است. لطفاً ابتدا تعرفه را تعریف کنید.";
                    return ApiResponse<DoctorScheduleDto>.Error(errorMessage, 400);
                }
            }

            schedule.DoctorId = dto.DoctorId;
            schedule.ShiftId = dto.ShiftId;
            schedule.DayOfWeek = dto.DayOfWeek;
            schedule.StartTime = TimeSpan.Parse(dto.StartTime);
            schedule.EndTime = TimeSpan.Parse(dto.EndTime);
            schedule.Count = dto.Count;
            schedule.ClinicId = dto.ClinicId;
            schedule.ServiceId = dto.ServiceId;

            await _repository.UpdateAsync(schedule, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("DoctorSchedule updated with ID: {DoctorScheduleId}", schedule.Id);

            var query = await _repository.GetQueryableAsync(cancellationToken);
            var updatedSchedule = await query
                .Include(s => s.Doctor)
                .Include(s => s.Shift)
                .Include(s => s.Clinic)
                .Include(s => s.Service)
                .FirstOrDefaultAsync(s => s.Id == schedule.Id, cancellationToken);

            var scheduleDto = MapToDto(updatedSchedule!);
            return ApiResponse<DoctorScheduleDto>.Success(scheduleDto, "برنامه پزشک با موفقیت به‌روزرسانی شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor schedule");
            return ApiResponse<DoctorScheduleDto>.Error("خطا در به‌روزرسانی برنامه پزشک", ex);
        }
    }

    public async Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("DoctorSchedule deleted with ID: {DoctorScheduleId}", id);
            return ApiResponse.Success("برنامه پزشک با موفقیت حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor schedule");
            return ApiResponse.Error("خطا در حذف برنامه پزشک", ex);
        }
    }

    /// <summary>
    /// تولید خودکار برنامه پزشک برای بازه زمانی مشخص
    /// این متد برای هر روز در بازه زمانی و برای هر شیفت انتخابی، یک برنامه ایجاد می‌کند
    /// زمان شروع و پایان از اطلاعات شیفت گرفته می‌شود
    /// </summary>
    /// <param name="dto">اطلاعات تولید برنامه شامل شناسه پزشک، تاریخ شروع، تاریخ پایان و لیست شناسه شیفت‌ها</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>لیست برنامه‌های تولید شده</returns>
    public async Task<ApiResponse<List<DoctorScheduleDto>>> GenerateScheduleAsync(GenerateScheduleDto dto, CancellationToken cancellationToken = default)
    {
        // بررسی وجود تعرفه برای پزشک، کلینیک و خدمت قبل از تولید برنامه‌ها
        var hasTariff = await CheckTariffExistsAsync(dto.DoctorId, dto.ClinicId, dto.ServiceId, cancellationToken);
        if (!hasTariff)
        {
            var errorMessage = dto.ClinicId.HasValue
                ? $"برای پزشک، کلینیک و خدمت مورد نظر تعرفه‌ای تعریف نشده است. لطفاً ابتدا تعرفه را تعریف کنید."
                : $"برای پزشک و خدمت مورد نظر تعرفه‌ای تعریف نشده است. لطفاً ابتدا تعرفه را تعریف کنید.";
            return ApiResponse<List<DoctorScheduleDto>>.Error(errorMessage, 400);
        }

        var schedules = new List<DoctorSchedule>();

        var shifts = new List<Shift>();
        foreach (var shiftId in dto.ShiftIds)
        {
            var shift = await _shiftRepository.GetByIdAsync(shiftId, cancellationToken);
            if (shift != null)
                shifts.Add(shift);
        }

        // برای هر روز هفته و هر شیفت، یک برنامه ایجاد می‌کنیم
        foreach (DayOfWeekModel dayOfWeek in Enum.GetValues(typeof(DayOfWeekModel)))
        {
            foreach (var shift in shifts)
            {
                var schedule = new DoctorSchedule
                {
                    DoctorId = dto.DoctorId,
                    ShiftId = shift.Id,
                    DayOfWeek = dayOfWeek,
                    StartTime = shift.StartTime,
                    EndTime = shift.EndTime,
                    Count = 1,
                    ClinicId = dto.ClinicId,
                    ServiceId = dto.ServiceId
                };

                schedules.Add(schedule);
                await _repository.AddAsync(schedule, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated {Count} schedules for Doctor ID: {DoctorId}", schedules.Count, dto.DoctorId);

        var query = await _repository.GetQueryableAsync(cancellationToken);
        var createdSchedules = await query
            .Include(s => s.Doctor)
            .Include(s => s.Shift)
            .Include(s => s.Clinic)
            .Include(s => s.Service)
            .Where(s => s.DoctorId == dto.DoctorId &&
                       dto.ShiftIds.Contains(s.ShiftId))
            .ToListAsync(cancellationToken);

        var scheduleDtos = createdSchedules.Select(MapToDto).ToList();
        return ApiResponse<List<DoctorScheduleDto>>.Success(scheduleDtos, $"{scheduleDtos.Count} برنامه با موفقیت تولید شد");
    }

    /// <summary>
    /// بررسی وجود تعرفه برای پزشک، کلینیک و خدمت
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
    /// <param name="serviceId">شناسه خدمت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>true اگر تعرفه وجود داشته باشد، در غیر این صورت false</returns>
    private async Task<bool> CheckTariffExistsAsync(int doctorId, int? clinicId, int serviceId, CancellationToken cancellationToken = default)
    {
        var tariffQuery = await _serviceTariffRepository.GetQueryableNoTrackingAsync(cancellationToken);

        if (clinicId.HasValue)
        {
            // بررسی تعرفه برای پزشک، کلینیک و خدمت مشخص
            // تعرفه می‌تواند مخصوص پزشک باشد (DoctorId == doctorId) یا عمومی برای کلینیک باشد (DoctorId == null)
            return await tariffQuery.AnyAsync(
                t => t.ServiceId == serviceId &&
                     t.ClinicId == clinicId.Value &&
                     (t.DoctorId == doctorId || t.DoctorId == null),
                cancellationToken);
        }
        else
        {
            // اگر کلینیک مشخص نشده باشد، بررسی تعرفه‌های عمومی برای پزشک و خدمت
            return await tariffQuery.AnyAsync(
                t => t.ServiceId == serviceId &&
                     t.DoctorId == doctorId,
                cancellationToken);
        }
    }

    /// <summary>
    /// تبدیل موجودیت DoctorSchedule به DTO
    /// این متد خصوصی برای تبدیل موجودیت به DTO با فرمت مناسب استفاده می‌شود
    /// </summary>
    /// <param name="schedule">موجودیت برنامه پزشک</param>
    /// <returns>DTO برنامه پزشک</returns>
    private DoctorScheduleDto MapToDto(DoctorSchedule schedule)
    {
        var dto = new DoctorScheduleDto
        {
            Id = schedule.Id,
            DoctorId = schedule.DoctorId,
            DoctorName = schedule.Doctor?.User != null
                ? $"{schedule.Doctor.User.FirstName} {schedule.Doctor.User.LastName}"
                : null,
            ShiftId = schedule.ShiftId,
            ShiftName = schedule.Shift?.Name,
            DayOfWeek = schedule.DayOfWeek,
            DayOfWeekName = schedule.DayOfWeek.ToPersianName(),
            StartTime = schedule.StartTime.ToString(@"hh\:mm"),
            EndTime = schedule.EndTime.ToString(@"hh\:mm"),
            Count = schedule.Count,
            ClinicId = schedule.ClinicId,
            ServiceId = schedule.ServiceId,
            ServiceName = schedule.Service?.Name,
            CreatedAt = schedule.CreatedAt
        };

        // Map Clinic if exists
        if (schedule.Clinic != null)
        {
            dto.Clinic = _mapper.Map<Clinics.Dto.ClinicDto>(schedule.Clinic);
        }

        return dto;
    }
}
