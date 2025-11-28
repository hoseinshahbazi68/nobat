using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Appointments;
using Nobat.Application.Appointments.Dto;
using Nobat.Application.Common;
using Nobat.Domain.Entities.Appointments;
using Nobat.Domain.Enums;
using Dto = Nobat.Application.Appointments.Dto;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت نوبت‌ها
/// این کنترلر API endpoints مربوط به مدیریت نوبت‌ها را ارائه می‌دهد
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class AppointmentsController : BaseController
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    /// <summary>
    /// ایجاد نوبت جدید
    /// </summary>
    /// <param name="dto">اطلاعات نوبت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نوبت ایجاد شده</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Appointment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Appointment>>> CreateAppointment(
        [FromBody] CreateAppointmentDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // تبدیل زمان شروع و پایان از string به TimeSpan
            var startTimeParts = dto.StartTime.Split(':');
            var endTimeParts = dto.EndTime.Split(':');

            if (startTimeParts.Length != 2 || endTimeParts.Length != 2)
            {
                return BadRequest(ApiResponse<Appointment>.Error("فرمت زمان نامعتبر است. فرمت صحیح: HH:mm", 400));
            }

            var startTime = new TimeSpan(int.Parse(startTimeParts[0]), int.Parse(startTimeParts[1]), 0);
            var endTime = new TimeSpan(int.Parse(endTimeParts[0]), int.Parse(endTimeParts[1]), 0);

            var appointment = new Appointment
            {
                DoctorScheduleId = dto.DoctorScheduleId,
                AppointmentDateTime = dto.AppointmentDateTime,
                ExpireDateTime = dto.ExpireDateTime,
                StartTime = startTime,
                EndTime = endTime,
                Status = AppointmentStatus.Booked
            };

            var response = await _appointmentService.CreateAsync(appointment, cancellationToken);

            if (response.Status)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (FormatException)
        {
            return BadRequest(ApiResponse<Appointment>.Error("فرمت زمان نامعتبر است. فرمت صحیح: HH:mm", 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, ApiResponse<Appointment>.Error("خطا در ایجاد نوبت", ex));
        }
    }

    /// <summary>
    /// تولید خودکار نوبت‌ها برای بازه زمانی مشخص
    /// </summary>
    /// <param name="startDate">تاریخ شروع (yyyy-MM-dd)</param>
    /// <param name="endDate">تاریخ پایان (yyyy-MM-dd)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>تعداد نوبت‌های ایجاد شده</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> GenerateAppointments(
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // اگر تاریخ شروع و پایان مشخص نشده باشد، از امروز تا 30 روز آینده استفاده می‌کنیم
            var start = string.IsNullOrEmpty(startDate)
                ? DateTime.Today
                : DateTime.Parse(startDate);

            var end = string.IsNullOrEmpty(endDate)
                ? DateTime.Today.AddDays(30)
                : DateTime.Parse(endDate);

            if (start > end)
            {
                return BadRequest(ApiResponse<int>.Error("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد", 400));
            }

            var response = await _appointmentService.GenerateAppointmentsAsync(start, end, cancellationToken);

            if (response.Status)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (FormatException)
        {
            return BadRequest(ApiResponse<int>.Error("فرمت تاریخ نامعتبر است. فرمت صحیح: yyyy-MM-dd", 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating appointments");
            return StatusCode(500, ApiResponse<int>.Error("خطا در تولید نوبت‌ها", ex));
        }
    }

    /// <summary>
    /// دریافت تعداد نوبت‌ها برای یک بازه تاریخ و پزشک مشخص
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="startDate">تاریخ شروع (yyyy-MM-dd)</param>
    /// <param name="endDate">تاریخ پایان (yyyy-MM-dd)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>لیست تعداد نوبت‌ها برای هر تاریخ</returns>
    [HttpGet("counts")]
    [ProducesResponseType(typeof(ApiResponse<List<Dto.AppointmentCountDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<Dto.AppointmentCountDto>>>> GetAppointmentCounts(
        [FromQuery] int doctorId,
        [FromQuery] string startDate,
        [FromQuery] string endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var start = DateTime.Parse(startDate);
            var end = DateTime.Parse(endDate);

            if (start > end)
            {
                return BadRequest(ApiResponse<List<Dto.AppointmentCountDto>>.Error("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد", 400));
            }

            var response = await _appointmentService.GetAppointmentCountsAsync(doctorId, start, end, cancellationToken);

            if (response.Status)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (FormatException)
        {
            return BadRequest(ApiResponse<List<Dto.AppointmentCountDto>>.Error("فرمت تاریخ نامعتبر است. فرمت صحیح: yyyy-MM-dd", 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment counts");
            return StatusCode(500, ApiResponse<List<Dto.AppointmentCountDto>>.Error("خطا در دریافت تعداد نوبت‌ها", ex));
        }
    }
}
