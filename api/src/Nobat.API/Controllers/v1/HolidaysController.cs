using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Schedules;
using Nobat.Application.Schedules.Dto;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت روزهای تعطیل
/// این کنترلر API endpoints مربوط به مدیریت روزهای تعطیل را ارائه می‌دهد
/// شامل عملیات CRUD کامل با احراز هویت و مجوزهای لازم
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class HolidaysController : BaseController
{
    /// <summary>
    /// سرویس روز تعطیل
    /// </summary>
    private readonly IHolidayService _holidayService;

    /// <summary>
    /// لاگر
    /// </summary>
    private readonly ILogger<HolidaysController> _logger;

    /// <summary>
    /// سازنده کنترلر روزهای تعطیل
    /// </summary>
    /// <param name="holidayService">سرویس روز تعطیل</param>
    /// <param name="logger">لاگر</param>
    public HolidaysController(IHolidayService holidayService, ILogger<HolidaysController> logger)
    {
        _holidayService = holidayService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت روز تعطیل بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه روز تعطیل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>روز تعطیل یافت شده</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HolidayDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var holiday = await _holidayService.GetByIdAsync(id, cancellationToken);
        if (holiday == null)
        {
            return NotFound();
        }
        return Ok(holiday);
    }

    /// <summary>
    /// دریافت لیست روزهای تعطیل با فیلتر و صفحه‌بندی
    /// </summary>
    /// <param name="sieveModel">مدل فیلتر و صفحه‌بندی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه صفحه‌بندی شده</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<HolidayDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<HolidayDto>>> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var result = await _holidayService.GetAllAsync(sieveModel, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// ایجاد روز تعطیل جدید
    /// </summary>
    /// <param name="dto">اطلاعات روز تعطیل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>روز تعطیل ایجاد شده</returns>
    [HttpPost]
    ////[Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HolidayDto>> Create([FromBody] CreateHolidayDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var holiday = await _holidayService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = holiday.Id, version = "1.0" }, holiday);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating holiday");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// به‌روزرسانی روز تعطیل
    /// </summary>
    /// <param name="id">شناسه روز تعطیل</param>
    /// <param name="dto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>روز تعطیل به‌روز شده</returns>
    [HttpPut("{id}")]
 //   //[Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HolidayDto>> Update(int id, [FromBody] UpdateHolidayDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest("عدم تطابق شناسه");
        }

        try
        {
            var holiday = await _holidayService.UpdateAsync(dto, cancellationToken);
            return Ok(holiday);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating holiday");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// حذف روز تعطیل
    /// </summary>
    /// <param name="id">شناسه روز تعطیل</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpDelete("{id}")]
  //  //[Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _holidayService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
