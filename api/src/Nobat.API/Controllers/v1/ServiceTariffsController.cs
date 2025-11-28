using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Services;
using Nobat.Application.Services.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت تعرفه‌های خدمات
/// این کنترلر API endpoints مربوط به مدیریت تعرفه‌های خدمات را ارائه می‌دهد
/// شامل عملیات CRUD کامل با احراز هویت و مجوزهای لازم
/// تعرفه‌ها بر اساس خدمت، بیمه و پزشک تعریف می‌شوند
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ServiceTariffsController : BaseController
{
    private readonly IServiceTariffService _serviceTariffService;
    private readonly ILogger<ServiceTariffsController> _logger;

    public ServiceTariffsController(IServiceTariffService serviceTariffService, ILogger<ServiceTariffsController> logger)
    {
        _serviceTariffService = serviceTariffService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _serviceTariffService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _serviceTariffService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateServiceTariffDto dto, CancellationToken cancellationToken)
    {
        var response = await _serviceTariffService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceTariffDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<ServiceTariffDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _serviceTariffService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    [HttpDelete("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _serviceTariffService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }
}
