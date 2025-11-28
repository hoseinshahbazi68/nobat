using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Appointments.Dto;
using Nobat.Application.Common;
using Nobat.Domain.Entities.Appointments;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Schedules;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IRepository<Appointment> appointmentRepository,
        IRepository<DoctorSchedule> doctorScheduleRepository,
        IRepository<Holiday> holidayRepository,
        IUnitOfWork unitOfWork,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _doctorScheduleRepository = doctorScheduleRepository;
        _holidayRepository = holidayRepository;
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
        var query = await _appointmentRepository.GetQueryableAsync(cancellationToken);
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
            var scheduleQuery = await _doctorScheduleRepository.GetQueryableAsync(cancellationToken);
            var schedules = await scheduleQuery
                .Include(s => s.Doctor)
                .Include(s => s.Shift)
                .ToListAsync(cancellationToken);

            // دریافت تمام روزهای تعطیل
            var holidayQuery = await _holidayRepository.GetQueryableAsync(cancellationToken);
            var holidays = await holidayQuery
                .Where(h => h.Date >= startDate.Date && h.Date <= endDate.Date)
                .Select(h => h.Date.Date)
                .ToListAsync(cancellationToken);

            // دریافت نوبت‌های موجود برای جلوگیری از ایجاد تکراری
            var appointmentQuery = await _appointmentRepository.GetQueryableAsync(cancellationToken);
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
                // تولید نوبت‌ها برای هر روز در بازه زمانی
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
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
                    var timeSlotDuration = schedule.EndTime - schedule.StartTime;
                    var slotDuration = timeSlotDuration.TotalMinutes / schedule.Count;

                    // ایجاد نوبت‌ها بر اساس Count
                    for (int i = 0; i < schedule.Count; i++)
                    {
                        var slotStartTime = schedule.StartTime.Add(TimeSpan.FromMinutes(slotDuration * i));
                        var slotEndTime = slotStartTime.Add(TimeSpan.FromMinutes(slotDuration));

                        var appointmentDate = date.Date + slotStartTime;
                        var appointmentExpireDate = date.Date + slotEndTime;

                        // بررسی اینکه آیا این نوبت قبلاً ایجاد شده است
                        var appointmentKey = (schedule.Id, appointmentDate.Date, slotStartTime);
                        if (existingAppointmentSet.Contains(appointmentKey))
                        {
                            continue;
                        }

                        // ایجاد نوبت جدید
                        // توجه: Status به صورت پیش‌فرض Booked است، اما نوبت‌های تولید شده باید Available باشند
                        // در حال حاضر AppointmentStatus فقط Booked, Cancelled, Completed دارد
                        // بنابراین نوبت‌های جدید را با Booked ایجاد می‌کنیم و بعداً می‌توانند رزرو شوند
                        var appointment = new Appointment
                        {
                            DoctorId = schedule.DoctorId,
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
            var query = await _appointmentRepository.GetQueryableAsync(cancellationToken);
            var appointments = await query
                .Where(a => a.DoctorId == doctorId &&
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
