using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Doctors;
using Nobat.Application.Doctors.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت تخصص‌ها
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SpecialtiesController : BaseController
{
    private readonly ISpecialtyService _specialtyService;
    private readonly ILogger<SpecialtiesController> _logger;

    /// <summary>
    /// سازنده کنترلر تخصص‌ها
    /// </summary>
    public SpecialtiesController(ISpecialtyService specialtyService, ILogger<SpecialtiesController> logger)
    {
        _specialtyService = specialtyService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت تخصص بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _specialtyService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست تخصص‌ها (عمومی - بدون نیاز به احراز هویت)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _specialtyService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ایجاد تخصص جدید
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateSpecialtyDto dto, CancellationToken cancellationToken)
    {
        var response = await _specialtyService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی تخصص
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSpecialtyDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<SpecialtyDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _specialtyService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف تخصص
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _specialtyService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست علائم پزشکی مرتبط با تخصص
    /// </summary>
    [HttpGet("{specialtyId}/medical-conditions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMedicalConditions(int specialtyId, CancellationToken cancellationToken)
    {
        var response = await _specialtyService.GetMedicalConditionsAsync(specialtyId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// افزودن علائم پزشکی به تخصص
    /// </summary>
    [HttpPost("{specialtyId}/medical-conditions/{medicalConditionId}")]
    [Authorize]
    public async Task<IActionResult> AddMedicalCondition(int specialtyId, int medicalConditionId, CancellationToken cancellationToken)
    {
        var response = await _specialtyService.AddMedicalConditionAsync(specialtyId, medicalConditionId, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(
                nameof(GetMedicalConditions),
                new { specialtyId = specialtyId, version = "1.0" },
                response);
        }

        return ToActionResult(response);
    }

    /// <summary>
    /// حذف علائم پزشکی از تخصص
    /// </summary>
    [HttpDelete("{specialtyId}/medical-conditions/{medicalConditionId}")]
    [Authorize]
    public async Task<IActionResult> RemoveMedicalCondition(int specialtyId, int medicalConditionId, CancellationToken cancellationToken)
    {
        var response = await _specialtyService.RemoveMedicalConditionAsync(specialtyId, medicalConditionId, cancellationToken);
        return ToActionResult(response);
    }
}
