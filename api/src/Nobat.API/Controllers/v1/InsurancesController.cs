using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Insurances;
using Nobat.Application.Insurances.Dto;
using Nobat.Application.Schedules;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت بیمه‌ها
/// این کنترلر API endpoints مربوط به مدیریت بیمه‌ها را ارائه می‌دهد
/// شامل عملیات CRUD کامل با احراز هویت و مجوزهای لازم
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class InsurancesController : BaseController
{
    private readonly IInsuranceService _insuranceService;
    private readonly ILogger<InsurancesController> _logger;

    public InsurancesController(IInsuranceService insuranceService, ILogger<InsurancesController> logger)
    {
        _insuranceService = insuranceService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _insuranceService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _insuranceService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateInsuranceDto dto, CancellationToken cancellationToken)
    {
        var response = await _insuranceService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInsuranceDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<InsuranceDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _insuranceService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    [HttpDelete("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _insuranceService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }
}
