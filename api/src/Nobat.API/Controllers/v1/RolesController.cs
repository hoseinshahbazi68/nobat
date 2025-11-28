using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Users;
using Nobat.Application.Users.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت نقش‌ها
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    /// <summary>
    /// سازنده کنترلر نقش‌ها
    /// </summary>
    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت نقش بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _roleService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست نقش‌ها
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _roleService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ایجاد نقش جدید
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto, CancellationToken cancellationToken)
    {
        var response = await _roleService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی نقش
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<RoleDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _roleService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف نقش
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _roleService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }
}
