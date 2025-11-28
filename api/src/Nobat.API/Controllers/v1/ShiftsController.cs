using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Schedules.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر شیفت
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ShiftsController : BaseController
{
    /// <summary>
    /// سرویس شیفت
    /// </summary>
    private readonly IShiftService _shiftService;

    /// <summary>
    /// لاگر
    /// </summary>
    private readonly ILogger<ShiftsController> _logger;

    /// <summary>
    /// سازنده کنترلر شیفت
    /// </summary>
    /// <param name="shiftService">سرویس شیفت</param>
    /// <param name="logger">لاگر</param>
    public ShiftsController(IShiftService shiftService, ILogger<ShiftsController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت شیفت بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت یافت شده</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var shift = await _shiftService.GetByIdAsync(id, cancellationToken);
            if (shift == null)
            {
                return ToActionResult(ApiResponse<ShiftDto>.Error("شیفت با شناسه مشخص شده یافت نشد", 404));
            }
            return ToActionResult(ApiResponse<ShiftDto>.Success(shift, "شیفت با موفقیت دریافت شد"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shift by id: {Id}", id);
            return ToActionResult(ApiResponse<ShiftDto>.Error("خطا در دریافت شیفت", ex));
        }
    }

    /// <summary>
    /// دریافت لیست صفحه‌بندی شده شیفت‌ها
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه صفحه‌بندی شده</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _shiftService.GetAllAsync(sieveModel, cancellationToken);
            return ToActionResult(ApiResponse<PagedResult<ShiftDto>>.Success(result, "لیست شیفت‌ها با موفقیت دریافت شد"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all shifts");
            return ToActionResult(ApiResponse<PagedResult<ShiftDto>>.Error("خطا در دریافت لیست شیفت‌ها", ex));
        }
    }

    /// <summary>
    /// ایجاد شیفت جدید
    /// </summary>
    /// <param name="dto">اطلاعات شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت ایجاد شده</returns>
    [HttpPost]
  //  //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateShiftDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var shift = await _shiftService.CreateAsync(dto, cancellationToken);
            var response = ApiResponse<ShiftDto>.Success(shift, "شیفت جدید با موفقیت ایجاد شد", 201);
            return CreatedAtAction(nameof(GetById), new { id = shift.Id, version = "1.0" }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift");
            return ToActionResult(ApiResponse<ShiftDto>.Error("خطا در ایجاد شیفت", ex));
        }
    }

    /// <summary>
    /// به‌روزرسانی شیفت
    /// </summary>
    /// <param name="id">شناسه شیفت</param>
    /// <param name="dto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>شیفت به‌روز شده</returns>
    [HttpPut("{id}")]
   // //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShiftDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return ToActionResult(ApiResponse<ShiftDto>.Error("عدم تطابق شناسه", 400, "شناسه موجود در URL با شناسه موجود در بدنه درخواست مطابقت ندارد"));
        }

        try
        {
            var shift = await _shiftService.UpdateAsync(dto, cancellationToken);
            return ToActionResult(ApiResponse<ShiftDto>.Success(shift, "شیفت با موفقیت به‌روزرسانی شد"));
        }
        catch (KeyNotFoundException)
        {
            return ToActionResult(ApiResponse<ShiftDto>.Error("شیفت با شناسه مشخص شده یافت نشد", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift");
            return ToActionResult(ApiResponse<ShiftDto>.Error("خطا در به‌روزرسانی شیفت", ex));
        }
    }

    /// <summary>
    /// حذف شیفت
    /// </summary>
    /// <param name="id">شناسه شیفت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpDelete("{id}")]
   // //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _shiftService.DeleteAsync(id, cancellationToken);
            return ToActionResult(ApiResponse.Success("شیفت با موفقیت حذف شد"));
        }
        catch (KeyNotFoundException)
        {
            return ToActionResult(ApiResponse.Error("شیفت با شناسه مشخص شده یافت نشد", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift with ID: {Id}", id);
            return ToActionResult(ApiResponse.Error("خطا در حذف شیفت", ex));
        }
    }
}
