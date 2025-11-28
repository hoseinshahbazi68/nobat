using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Doctors;
using Nobat.Application.Doctors.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت اطلاعات ویزیت پزشک
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DoctorVisitInfosController : BaseController
{
    private readonly IDoctorVisitInfoService _visitInfoService;
    private readonly ILogger<DoctorVisitInfosController> _logger;

    public DoctorVisitInfosController(
        IDoctorVisitInfoService visitInfoService,
        ILogger<DoctorVisitInfosController> logger)
    {
        _visitInfoService = visitInfoService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لیست اطلاعات ویزیت با فیلتر و صفحه‌بندی
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        var response = await _visitInfoService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت اطلاعات ویزیت بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var response = await _visitInfoService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت اطلاعات ویزیت بر اساس شناسه پزشک
    /// </summary>
    [HttpGet("by-doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctorId(int doctorId, CancellationToken cancellationToken = default)
    {
        var response = await _visitInfoService.GetByDoctorIdAsync(doctorId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ایجاد اطلاعات ویزیت جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorVisitInfoDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _visitInfoService.CreateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی اطلاعات ویزیت
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorVisitInfoDto dto, CancellationToken cancellationToken = default)
    {
        dto.Id = id;
        var response = await _visitInfoService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف اطلاعات ویزیت
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var response = await _visitInfoService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }
}
