using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Doctors;
using Nobat.Application.Doctors.Dto;
using Nobat.Application.Schedules;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت برنامه‌های پزشکان
/// این کنترلر API endpoints مربوط به مدیریت برنامه‌های پزشکان را ارائه می‌دهد
/// شامل عملیات CRUD کامل، دریافت برنامه هفتگی و تولید خودکار برنامه
/// با احراز هویت و مجوزهای لازم
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DoctorSchedulesController : BaseController
{
    private readonly IDoctorScheduleService _doctorScheduleService;
    private readonly ILogger<DoctorSchedulesController> _logger;

    public DoctorSchedulesController(IDoctorScheduleService doctorScheduleService, ILogger<DoctorSchedulesController> logger)
    {
        _doctorScheduleService = doctorScheduleService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _doctorScheduleService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _doctorScheduleService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت برنامه هفتگی پزشک
    /// این endpoint برنامه پزشک را برای یک هفته (7 روز) از تاریخ شروع مشخص شده برمی‌گرداند
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="weekStartDate">تاریخ شروع هفته (فرمت: yyyy-MM-dd)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>برنامه هفتگی پزشک</returns>
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklySchedule([FromQuery] int doctorId, [FromQuery] string weekStartDate, CancellationToken cancellationToken)
    {
        var response = await _doctorScheduleService.GetWeeklyScheduleAsync(doctorId, weekStartDate, cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDoctorScheduleDto dto, CancellationToken cancellationToken)
    {
        var response = await _doctorScheduleService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorScheduleDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<DoctorScheduleDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _doctorScheduleService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    [HttpDelete("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _doctorScheduleService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// تولید خودکار برنامه پزشک برای بازه زمانی مشخص
    /// این endpoint برای هر روز در بازه زمانی و برای هر شیفت انتخابی، یک برنامه ایجاد می‌کند
    /// زمان شروع و پایان از اطلاعات شیفت گرفته می‌شود
    /// </summary>
    /// <param name="dto">اطلاعات تولید برنامه شامل شناسه پزشک، تاریخ شروع، تاریخ پایان و لیست شناسه شیفت‌ها</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>لیست برنامه‌های تولید شده</returns>
    [HttpPost("generate")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateSchedule([FromBody] GenerateScheduleDto dto, CancellationToken cancellationToken)
    {
        var response = await _doctorScheduleService.GenerateScheduleAsync(dto, cancellationToken);
        return ToActionResult(response);
    }
}
