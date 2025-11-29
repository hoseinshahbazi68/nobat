using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Locations;
using Nobat.Application.Locations.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت شهرها
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CitiesController : BaseController
{
    private readonly ICityService _cityService;
    private readonly ILogger<CitiesController> _logger;

    public CitiesController(ICityService cityService, ILogger<CitiesController> logger)
    {
        _cityService = cityService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت شهر بر اساس شناسه بدون نیاز به احراز هویت (برای صفحه ثبت‌نام)
    /// </summary>
    [HttpGet("public/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdPublic(int id, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست شهرها بدون نیاز به احراز هویت (برای صفحه ثبت‌نام)
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPublic([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet("province/{provinceId}")]
    public async Task<IActionResult> GetByProvinceId(int provinceId, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetByProvinceIdAsync(provinceId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت شهرهای یک استان بدون نیاز به احراز هویت (برای صفحه اصلی)
    /// </summary>
    [HttpGet("public/province/{provinceId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProvinceIdPublic(int provinceId, CancellationToken cancellationToken)
    {
        var response = await _cityService.GetByProvinceIdAsync(provinceId, cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCityDto dto, CancellationToken cancellationToken)
    {
        var response = await _cityService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCityDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<CityDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _cityService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    [HttpDelete("{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _cityService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }
}
