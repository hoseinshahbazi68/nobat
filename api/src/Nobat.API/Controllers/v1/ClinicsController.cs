using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Clinics;
using Nobat.Application.Clinics.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت کلینیک‌ها
/// این کنترلر API endpoints مربوط به مدیریت کلینیک‌ها را ارائه می‌دهد
/// شامل عملیات CRUD کامل با احراز هویت و مجوزهای لازم
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ClinicsController : BaseController
{
    /// <summary>
    /// سرویس کلینیک
    /// </summary>
    private readonly IClinicService _clinicService;

    /// <summary>
    /// لاگر
    /// </summary>
    private readonly ILogger<ClinicsController> _logger;

    /// <summary>
    /// سازنده کنترلر کلینیک‌ها
    /// </summary>
    /// <param name="clinicService">سرویس کلینیک</param>
    /// <param name="logger">لاگر</param>
    public ClinicsController(IClinicService clinicService, ILogger<ClinicsController> logger)
    {
        _clinicService = clinicService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت کلینیک بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه کلینیک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>کلینیک یافت شده</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _clinicService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست کلینیک‌ها با فیلتر و صفحه‌بندی
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه صفحه‌بندی شده</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _clinicService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ایجاد کلینیک جدید
    /// </summary>
    /// <param name="dto">اطلاعات کلینیک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>کلینیک ایجاد شده</returns>
    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateClinicDto dto, CancellationToken cancellationToken)
    {
        var response = await _clinicService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی کلینیک
    /// </summary>
    /// <param name="id">شناسه کلینیک</param>
    /// <param name="dto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>کلینیک به‌روز شده</returns>
    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClinicDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<ClinicDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _clinicService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف کلینیک
    /// </summary>
    /// <param name="id">شناسه کلینیک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpDelete("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _clinicService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست ساده کلینیک‌ها (فقط نام و شناسه)
    /// برای استفاده در dropdown و لیست‌های ساده
    /// </summary>
    /// <param name="searchTerm">عبارت جستجو برای فیلتر کردن بر اساس نام</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>لیست ساده کلینیک‌ها</returns>
    [HttpGet("simple")]
    public async Task<IActionResult> GetSimpleList([FromQuery] string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var response = await _clinicService.GetSimpleListAsync(searchTerm, cancellationToken);
        return ToActionResult(response);
    }
}
