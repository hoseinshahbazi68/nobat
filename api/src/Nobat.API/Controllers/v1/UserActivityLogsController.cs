using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Users;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت لاگ فعالیت‌های کاربران
/// این کنترلر API endpoints مربوط به مشاهده لاگ فعالیت‌های کاربران را ارائه می‌دهد
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UserActivityLogsController : BaseController
{
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly ILogger<UserActivityLogsController> _logger;

    /// <summary>
    /// سازنده کنترلر لاگ فعالیت‌های کاربران
    /// </summary>
    public UserActivityLogsController(IUserActivityLogService userActivityLogService, ILogger<UserActivityLogsController> logger)
    {
        _userActivityLogService = userActivityLogService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لاگ فعالیت بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _userActivityLogService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست تمام لاگ فعالیت‌های کاربران
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _userActivityLogService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لاگ فعالیت‌های یک کاربر خاص
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId, [FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _userActivityLogService.GetByUserIdAsync(userId, sieveModel, cancellationToken);
        return ToActionResult(response);
    }
}
