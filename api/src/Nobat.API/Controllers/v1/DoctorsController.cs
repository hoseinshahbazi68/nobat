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
/// کنترلر مدیریت پزشکان
/// این کنترلر API endpoints مربوط به مدیریت پزشکان را ارائه می‌دهد
/// شامل عملیات CRUD کامل با احراز هویت و مجوزهای لازم
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DoctorsController : BaseController
{
    /// <summary>
    /// سرویس پزشک
    /// </summary>
    private readonly IDoctorService _doctorService;

    /// <summary>
    /// لاگر
    /// </summary>
    private readonly ILogger<DoctorsController> _logger;

    /// <summary>
    /// سازنده کنترلر پزشکان
    /// </summary>
    /// <param name="doctorService">سرویس پزشک</param>
    /// <param name="logger">لاگر</param>
    public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لیست پزشکان با فیلتر و صفحه‌بندی
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="clinicId">شناسه مرکز (اختیاری - برای فیلتر بر اساس مرکز)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه صفحه‌بندی شده</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, [FromQuery] int? clinicId = null, CancellationToken cancellationToken = default)
    {
        var response = await _doctorService.GetAllAsync(sieveModel, clinicId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// جستجوی پزشکان بر اساس نام پزشک یا نام کلینیک
    /// </summary>
    /// <param name="query">متن جستجو (اختیاری)</param>
    /// <param name="clinicName">نام کلینیک (اختیاری)</param>
    /// <param name="doctorName">نام پزشک (اختیاری)</param>
    /// <param name="page">شماره صفحه (پیش‌فرض: 1)</param>
    /// <param name="pageSize">تعداد آیتم در هر صفحه (پیش‌فرض: 10)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه جستجو</returns>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string? query = null,
        [FromQuery] string? clinicName = null,
        [FromQuery] string? doctorName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _doctorService.SearchAsync(query, clinicName, doctorName, page, pageSize, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// جستجوی پزشک بر اساس کد نظام پزشکی
    /// </summary>
    /// <param name="medicalCode">کد نظام پزشکی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پزشک یافت شده</returns>
    [HttpGet("by-medical-code/{medicalCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByMedicalCode(string medicalCode, CancellationToken cancellationToken)
    {
        var response = await _doctorService.GetByMedicalCodeAsync(medicalCode, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// جستجوی پزشکان بر اساس علائم پزشکی
    /// </summary>
    /// <param name="medicalConditionName">نام علائم پزشکی</param>
    /// <param name="page">شماره صفحه (پیش‌فرض: 1)</param>
    /// <param name="pageSize">تعداد آیتم در هر صفحه (پیش‌فرض: 10)</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه جستجو</returns>
    [HttpGet("by-medical-condition")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchByMedicalCondition(
        [FromQuery] string medicalConditionName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _doctorService.SearchByMedicalConditionAsync(medicalConditionName, page, pageSize, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت پزشک بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه پزشک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پزشک یافت شده</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _doctorService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ایجاد پزشک جدید
    /// </summary>
    /// <param name="dto">اطلاعات پزشک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پزشک ایجاد شده</returns>
    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto, CancellationToken cancellationToken)
    {
        var response = await _doctorService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    /// <summary>
    /// ایجاد پزشک جدید با فایل
    /// </summary>
    /// <param name="dto">اطلاعات پزشک با فایل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پزشک ایجاد شده</returns>
    [HttpPost("with-file")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateWithFile([FromForm] CreateDoctorWithFileDto dto, CancellationToken cancellationToken)
    {
        var response = await _doctorService.CreateWithFileAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی پزشک
    /// </summary>
    /// <param name="id">شناسه پزشک</param>
    /// <param name="dto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پزشک به‌روز شده</returns>
    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<DoctorDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _doctorService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی پزشک با فایل
    /// </summary>
    /// <param name="id">شناسه پزشک</param>
    /// <param name="dto">اطلاعات به‌روزرسانی با فایل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پزشک به‌روز شده</returns>
    [HttpPut("{id}/with-file")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateWithFile(int id, [FromForm] UpdateDoctorWithFileDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<DoctorDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _doctorService.UpdateWithFileAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف پزشک
    /// </summary>
    /// <param name="id">شناسه پزشک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpDelete("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _doctorService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }
}
