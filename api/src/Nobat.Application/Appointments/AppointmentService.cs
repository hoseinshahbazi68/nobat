using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Appointments.Dto;
using Nobat.Application.Common;
using Nobat.Domain.Entities.Appointments;
using Nobat.Domain.Entities.Clinics;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Schedules;
using Nobat.Domain.Entities.Services;
using Nobat.Domain.Enums;
using Nobat.Domain.Interfaces;

namespace Nobat.Application.Appointments;

/// <summary>
/// سرویس نوبت
/// این سرویس عملیات مربوط به نوبت‌ها را مدیریت می‌کند
/// شامل ایجاد و تولید خودکار نوبت‌ها
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRepository<DoctorSchedule> _doctorScheduleRepository;
    private readonly IRepository<Holiday> _holidayRepository;
    private readonly IRepository<ServiceTariff> _serviceTariffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IRepository<Appointment> appointmentRepository,
        IRepository<DoctorSchedule> doctorScheduleRepository,
        IRepository<Holiday> holidayRepository,
        IRepository<ServiceTariff> serviceTariffRepository,
        IUnitOfWork unitOfWork,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _doctorScheduleRepository = doctorScheduleRepository;
        _holidayRepository = holidayRepository;
        _serviceTariffRepository = serviceTariffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// ایجاد نوبت جدید
    /// </summary>
    public async Task<ApiResponse<Appointment>> CreateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        try
        {
            // دریافت برنامه پزشک برای دسترسی به کلینیک
            var doctorSchedule = await _doctorScheduleRepository.GetByIdAsync(appointment.DoctorScheduleId, cancellationToken);
            if (doctorSchedule == null)
            {
                return ApiResponse<Appointment>.Error("برنامه پزشک یافت نشد", 404);
            }

            // بررسی وجود تعرفه برای پزشک، کلینیک و خدمت
            var hasTariff = await CheckTariffExistsAsync(doctorSchedule.DoctorId, doctorSchedule.ClinicId, doctorSchedule.ServiceId, cancellationToken);
            if (!hasTariff)
            {
                var errorMessage = doctorSchedule.ClinicId.HasValue
                    ? $"برای پزشک، کلینیک و خدمت مورد نظر تعرفه‌ای تعریف نشده است. لطفاً ابتدا تعرفه را تعریف کنید."
                    : $"برای پزشک و خدمت مورد نظر تعرفه‌ای تعریف نشده است. لطفاً ابتدا تعرفه را تعریف کنید.";
                return ApiResponse<Appointment>.Error(errorMessage, 400);
            }

            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Appointment created with ID: {AppointmentId} for DoctorSchedule: {DoctorScheduleId}",
                appointment.Id, appointment.DoctorScheduleId);

            return ApiResponse<Appointment>.Success(appointment, "نوبت با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return ApiResponse<Appointment>.Error("خطا در ایجاد نوبت", ex);
        }
    }

    /// <summary>
    /// بررسی وجود نوبت برای یک برنامه و تاریخ مشخص
    /// </summary>
    public async Task<bool> ExistsAsync(int doctorScheduleId, DateTime appointmentDateTime, CancellationToken cancellationToken = default)
    {
        var query = await _appointmentRepository.GetQueryableNoTrackingAsync(cancellationToken);
        return await query.AnyAsync(
            a => a.DoctorScheduleId == doctorScheduleId &&
                 a.AppointmentDateTime.Date == appointmentDateTime.Date &&
                 a.StartTime == appointmentDateTime.TimeOfDay,
            cancellationToken);
    }

    /// <summary>
    /// تولید خودکار نوبت‌ها برای یک بازه زمانی
    /// </summary>
    public async Task<ApiResponse<int>> GenerateAppointmentsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var createdCount = 0;

            // دریافت تمام برنامه‌های پزشکان
            var scheduleQuery = await _doctorScheduleRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var schedules = await scheduleQuery
                .Include(s => s.Doctor)
                .Include(s => s.Shift)
                .Include(s => s.Clinic)
                .ToListAsync(cancellationToken);

            // دریافت تمام روزهای تعطیل
            var holidayQuery = await _holidayRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var holidays = await holidayQuery
                .Where(h => h.Date >= startDate.Date && h.Date <= endDate.Date)
                .Select(h => h.Date.Date)
                .ToListAsync(cancellationToken);

            // دریافت نوبت‌های موجود برای جلوگیری از ایجاد تکراری
            var appointmentQuery = await _appointmentRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var existingAppointments = await appointmentQuery
                .Where(a => a.AppointmentDateTime >= startDate && a.AppointmentDateTime <= endDate)
                .Select(a => new { a.DoctorScheduleId, a.AppointmentDateTime.Date, a.StartTime })
                .ToListAsync(cancellationToken);

            var existingAppointmentSet = existingAppointments
                .Select(a => (a.DoctorScheduleId, Date: a.Date, StartTime: a.StartTime))
                .ToHashSet();

            // برای هر برنامه پزشک
            foreach (var schedule in schedules)
            {
                // بررسی وجود تعرفه برای پزشک، کلینیک و خدمت قبل از تولید نوبت‌ها
                var hasTariff = await CheckTariffExistsAsync(schedule.DoctorId, schedule.ClinicId, schedule.ServiceId, cancellationToken);
                if (!hasTariff)
                {
                    _logger.LogWarning(
                        "Skipping schedule {ScheduleId} for doctor {DoctorId}, clinic {ClinicId} and service {ServiceId} - no tariff defined",
                        schedule.Id, schedule.DoctorId, schedule.ClinicId, schedule.ServiceId);
                    continue;
                }

                // دریافت VisitDuration از ServiceTariff
                var visitDuration = await GetVisitDurationAsync(schedule.DoctorId, schedule.ClinicId, schedule.ServiceId, cancellationToken);

                // بررسی محدودیت AppointmentGenerationDays برای کلینیک
                // تاریخ شروع باید از امروز باشد
                DateTime effectiveStartDate = startDate < DateTime.Today ? DateTime.Today : startDate.Date;
                DateTime effectiveEndDate = endDate;

                if (schedule.ClinicId.HasValue && schedule.Clinic != null)
                {
                    var appointmentGenerationDays = schedule.Clinic.AppointmentGenerationDays;
                    if (appointmentGenerationDays.HasValue)
                    {
                        var maxAllowedDate = DateTime.Today.AddDays(appointmentGenerationDays.Value);
                        if (endDate > maxAllowedDate)
                        {
                            effectiveEndDate = maxAllowedDate;
                            _logger.LogInformation(
                                "Limiting appointment generation for clinic {ClinicId} (Schedule {ScheduleId}) to {MaxDate} based on AppointmentGenerationDays: {Days}",
                                schedule.ClinicId, schedule.Id, maxAllowedDate, appointmentGenerationDays.Value);
                        }
                    }
                }

                // اگر تاریخ شروع موثر بعد از تاریخ پایان موثر باشد، این schedule را رد می‌کنیم
                if (effectiveStartDate > effectiveEndDate)
                {
                    _logger.LogWarning(
                        "Skipping schedule {ScheduleId} - effective start date {StartDate} is after effective end date {EndDate}",
                        schedule.Id, effectiveStartDate, effectiveEndDate);
                    continue;
                }

                // تولید نوبت‌ها برای هر روز در بازه زمانی
                for (var date = effectiveStartDate; date <= effectiveEndDate; date = date.AddDays(1))
                {
                    // بررسی اینکه آیا این روز تعطیل است
                    if (holidays.Contains(date))
                    {
                        continue;
                    }

                    // تبدیل روز هفته میلادی به شمسی
                    var gregorianDayOfWeek = date.DayOfWeek;
                    var persianDayOfWeek = ConvertToPersianDayOfWeek(gregorianDayOfWeek);

                    // بررسی اینکه آیا این روز با DayOfWeek برنامه مطابقت دارد
                    if (schedule.DayOfWeek != persianDayOfWeek)
                    {
                        continue;
                    }

                    // محاسبه زمان نوبت
                    var appointmentDateTime = date.Date + schedule.StartTime;
                    var expireDateTime = date.Date + schedule.EndTime;

                    // محاسبه فاصله زمانی بین نوبت‌ها
                    // اگر VisitDuration از ServiceTariff موجود باشد، از آن استفاده می‌کنیم
                    // در غیر این صورت از منطق قبلی (تقسیم زمان بر Count) استفاده می‌کنیم
                    double slotDuration;
                    int appointmentCount;
                    TimeSpan currentStartTime = schedule.StartTime;

                    if (visitDuration.HasValue && visitDuration.Value > 0)
                    {
                        // استفاده از VisitDuration از ServiceTariff
                        slotDuration = visitDuration.Value;
                        var totalDuration = (schedule.EndTime - schedule.StartTime).TotalMinutes;
                        appointmentCount = (int)Math.Floor(totalDuration / slotDuration);

                        // اگر appointmentCount صفر یا منفی باشد، حداقل یک نوبت ایجاد می‌کنیم
                        if (appointmentCount <= 0)
                        {
                            appointmentCount = 1;
                        }
                    }
                    else
                    {
                        // استفاده از منطق قبلی (تقسیم زمان بر Count)
                        var timeSlotDuration = schedule.EndTime - schedule.StartTime;
                        slotDuration = timeSlotDuration.TotalMinutes / schedule.Count;
                        appointmentCount = schedule.Count;
                    }

                    // ایجاد نوبت‌ها
                    // هر نوبت از پایان نوبت قبلی شروع می‌شود (بدون فاصله)
                    for (int i = 0; i < appointmentCount; i++)
                    {
                        // بررسی اینکه آیا زمان شروع از EndTime تجاوز کرده است
                        if (currentStartTime >= schedule.EndTime)
                        {
                            break;
                        }

                        var slotStartTime = currentStartTime;
                        var slotEndTime = slotStartTime.Add(TimeSpan.FromMinutes(slotDuration));

                        // بررسی اینکه آیا نوبت از EndTime تجاوز نمی‌کند
                        bool isLastAppointment = false;
                        if (slotEndTime > schedule.EndTime)
                        {
                            // اگر نوبت از EndTime تجاوز می‌کند، آن را تا EndTime محدود می‌کنیم
                            slotEndTime = schedule.EndTime;
                            isLastAppointment = true;
                        }

                        // بررسی اینکه آیا زمان شروع و پایان معتبر هستند
                        if (slotStartTime >= slotEndTime)
                        {
                            break;
                        }

                        var appointmentDate = date.Date + slotStartTime;
                        var appointmentExpireDate = date.Date + slotEndTime;

                        // بررسی اینکه آیا این نوبت قبلاً ایجاد شده است
                        var appointmentKey = (schedule.Id, appointmentDate.Date, slotStartTime);
                        if (existingAppointmentSet.Contains(appointmentKey))
                        {
                            // برای نوبت بعدی، زمان شروع را برابر با پایان این نوبت قرار می‌دهیم
                            currentStartTime = slotEndTime;
                            continue;
                        }

                        // ایجاد نوبت جدید
                        // توجه: Status به صورت پیش‌فرض Booked است، اما نوبت‌های تولید شده باید Available باشند
                        // در حال حاضر AppointmentStatus فقط Booked, Cancelled, Completed دارد
                        // بنابراین نوبت‌های جدید را با Booked ایجاد می‌کنیم و بعداً می‌توانند رزرو شوند
                        var appointment = new Appointment
                        {
                            DoctorScheduleId = schedule.Id,
                            AppointmentDateTime = appointmentDate,
                            ExpireDateTime = appointmentExpireDate,
                            StartTime = slotStartTime,
                            EndTime = slotEndTime,
                            Status = AppointmentStatus.Booked // نوبت‌های تولید شده آماده رزرو هستند
                        };

                        await _appointmentRepository.AddAsync(appointment, cancellationToken);
                        createdCount++;

                        // اضافه کردن به مجموعه برای جلوگیری از ایجاد تکراری در همان اجرا
                        existingAppointmentSet.Add(appointmentKey);

                        // برای نوبت بعدی، زمان شروع را برابر با پایان این نوبت قرار می‌دهیم
                        currentStartTime = slotEndTime;

                        // اگر این آخرین نوبت بود (به EndTime رسیدیم)، از حلقه خارج می‌شویم
                        if (isLastAppointment)
                        {
                            break;
                        }
                    }
                }
            }

            // ذخیره تغییرات
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated {Count} appointments from {StartDate} to {EndDate}",
                createdCount, startDate, endDate);

            return ApiResponse<int>.Success(createdCount, $"تعداد {createdCount} نوبت با موفقیت ایجاد شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating appointments from {StartDate} to {EndDate}", startDate, endDate);
            return ApiResponse<int>.Error("خطا در تولید نوبت‌ها", ex);
        }
    }

    /// <summary>
    /// دریافت تعداد نوبت‌ها برای یک بازه تاریخ و پزشک مشخص
    /// </summary>
    public async Task<ApiResponse<List<Dto.AppointmentCountDto>>> GetAppointmentCountsAsync(int doctorId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _appointmentRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var appointments = await query
                .Include(a => a.DoctorSchedule)
                .Where(a => a.DoctorSchedule.DoctorId == doctorId &&
                           a.AppointmentDateTime.Date >= startDate.Date &&
                           a.AppointmentDateTime.Date <= endDate.Date)
                .ToListAsync(cancellationToken);

            // گروه‌بندی بر اساس تاریخ
            var groupedByDate = appointments
                .GroupBy(a => a.AppointmentDateTime.Date)
                .Select(g => new Dto.AppointmentCountDto
                {
                    Date = g.Key,
                    TotalCount = g.Count(),
                    BookedCount = g.Count(a => a.Status == AppointmentStatus.Booked),
                    AvailableCount = g.Count(a => a.Status != AppointmentStatus.Booked && a.Status != AppointmentStatus.Cancelled)
                })
                .ToList();

            return ApiResponse<List<Dto.AppointmentCountDto>>.Success(groupedByDate, "تعداد نوبت‌ها با موفقیت دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment counts for doctor {DoctorId} from {StartDate} to {EndDate}", doctorId, startDate, endDate);
            return ApiResponse<List<Dto.AppointmentCountDto>>.Error("خطا در دریافت تعداد نوبت‌ها", ex);
        }
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
    /// دریافت مدت زمان ویزیت (VisitDuration) از ServiceTariff
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
    /// <param name="serviceId">شناسه خدمت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مدت زمان ویزیت به دقیقه یا null اگر پیدا نشد</returns>
    private async Task<int?> GetVisitDurationAsync(int doctorId, int? clinicId, int serviceId, CancellationToken cancellationToken = default)
    {
        var tariffQuery = await _serviceTariffRepository.GetQueryableNoTrackingAsync(cancellationToken);

        if (clinicId.HasValue)
        {
            // اولویت با تعرفه مخصوص پزشک است، سپس تعرفه عمومی کلینیک
            var doctorSpecificTariff = await tariffQuery
                .Where(t => t.ServiceId == serviceId &&
                           t.ClinicId == clinicId.Value &&
                           t.DoctorId == doctorId &&
                           t.VisitDuration.HasValue)
                .FirstOrDefaultAsync(cancellationToken);

            if (doctorSpecificTariff != null)
            {
                return doctorSpecificTariff.VisitDuration;
            }

            // اگر تعرفه مخصوص پزشک پیدا نشد، تعرفه عمومی کلینیک را بررسی می‌کنیم
            var clinicTariff = await tariffQuery
                .Where(t => t.ServiceId == serviceId &&
                           t.ClinicId == clinicId.Value &&
                           t.DoctorId == null &&
                           t.VisitDuration.HasValue)
                .FirstOrDefaultAsync(cancellationToken);

            return clinicTariff?.VisitDuration;
        }
        else
        {
            // اگر کلینیک مشخص نشده باشد، تعرفه‌های عمومی برای پزشک و خدمت را بررسی می‌کنیم
            var tariff = await tariffQuery
                .Where(t => t.ServiceId == serviceId &&
                           t.DoctorId == doctorId &&
                           t.VisitDuration.HasValue)
                .FirstOrDefaultAsync(cancellationToken);

            return tariff?.VisitDuration;
        }
    }

    /// <summary>
    /// تبدیل روز هفته میلادی به شمسی
    /// </summary>
    private DayOfWeekModel ConvertToPersianDayOfWeek(DayOfWeek gregorianDayOfWeek)
    {
        // تبدیل: 0 = یکشنبه میلادی -> 1 = یکشنبه شمسی
        // 1 = دوشنبه میلادی -> 2 = دوشنبه شمسی
        // ...
        // 6 = شنبه میلادی -> 0 = شنبه شمسی
        return gregorianDayOfWeek == DayOfWeek.Saturday
            ? DayOfWeekModel.Saturday
            : (DayOfWeekModel)((int)gregorianDayOfWeek + 1);
    }
}
