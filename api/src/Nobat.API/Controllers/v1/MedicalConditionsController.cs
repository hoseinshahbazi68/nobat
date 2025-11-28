using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Doctors;
using Nobat.Application.Doctors.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت علائم پزشکی
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class MedicalConditionsController : BaseController
{
    private readonly IMedicalConditionService _medicalConditionService;
    private readonly ILogger<MedicalConditionsController> _logger;

    /// <summary>
    /// سازنده کنترلر علائم پزشکی
    /// </summary>
    public MedicalConditionsController(IMedicalConditionService medicalConditionService, ILogger<MedicalConditionsController> logger)
    {
        _medicalConditionService = medicalConditionService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت علائم بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _medicalConditionService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست علائم (عمومی - بدون نیاز به احراز هویت)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _medicalConditionService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// جستجوی علائم بر اساس نام
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string query, CancellationToken cancellationToken)
    {
        var response = await _medicalConditionService.SearchAsync(query, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ایجاد علائم جدید
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateMedicalConditionDto dto, CancellationToken cancellationToken)
    {
        var response = await _medicalConditionService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی علائم
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicalConditionDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<MedicalConditionDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _medicalConditionService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف علائم
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _medicalConditionService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }
}
