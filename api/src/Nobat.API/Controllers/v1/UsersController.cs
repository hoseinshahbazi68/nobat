using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Users;
using Nobat.Application.Users.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت کاربران
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// سازنده کنترلر کاربران
    /// </summary>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت کاربر بر اساس کد ملی
    /// </summary>
    [HttpGet("by-national-code/{nationalCode}")]
    public async Task<IActionResult> GetByNationalCode(string nationalCode, CancellationToken cancellationToken)
    {
        var response = await _userService.GetByNationalCodeAsync(nationalCode, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت کاربر بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _userService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست کاربران
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _userService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ایجاد کاربر جدید
    /// </summary>
    [HttpPost]
   // //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var response = await _userService.CreateAsync(dto, cancellationToken);

        if (response.Status && response.Data != null)
        {
            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id, version = "1.0" }, response);
        }

        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی کاربر
    /// </summary>
    [HttpPut("{id}")]
   // //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            var errorResponse = ApiResponse<UserDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد");
            return ToActionResult(errorResponse);
        }

        var response = await _userService.UpdateAsync(dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف کاربر
    /// </summary>
    [HttpDelete("{id}")]
   // //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var response = await _userService.DeleteAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست کلینیک‌های کاربر
    /// </summary>
    [HttpGet("{id}/clinics")]
    public async Task<IActionResult> GetUserClinics(int id, CancellationToken cancellationToken)
    {
        var response = await _userService.GetUserClinicsAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// اختصاص کلینیک به کاربر
    /// </summary>
    [HttpPost("{id}/clinics")]
    public async Task<IActionResult> AssignClinicToUser(int id, [FromBody] AssignClinicToUserDto dto, CancellationToken cancellationToken)
    {
        var response = await _userService.AssignClinicToUserAsync(id, dto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف دسترسی کاربر به کلینیک
    /// </summary>
    [HttpDelete("{id}/clinics/{clinicId}")]
    public async Task<IActionResult> RemoveClinicFromUser(int id, int clinicId, CancellationToken cancellationToken)
    {
        var response = await _userService.RemoveClinicFromUserAsync(id, clinicId, cancellationToken);
        return ToActionResult(response);
    }
}
