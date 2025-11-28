using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.QueryLogs;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت لاگ کوئری‌های دیتابیس
/// این کنترلر API endpoints مربوط به مشاهده و مدیریت لاگ کوئری‌های سنگین را ارائه می‌دهد
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "Admin")]
public class QueryLogsController : BaseController
{
    private readonly IQueryLogService _queryLogService;
    private readonly ILogger<QueryLogsController> _logger;

    /// <summary>
    /// سازنده کنترلر لاگ کوئری‌ها
    /// </summary>
    public QueryLogsController(IQueryLogService queryLogService, ILogger<QueryLogsController> logger)
    {
        _queryLogService = queryLogService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لاگ کوئری بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _queryLogService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست کوئری‌های سنگین
    /// </summary>
    [HttpGet("heavy")]
    public async Task<IActionResult> GetHeavyQueries([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _queryLogService.GetHeavyQueriesAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست تمام لاگ کوئری‌ها
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _queryLogService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت آمار کوئری‌های سنگین
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var response = await _queryLogService.GetStatisticsAsync(cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// حذف لاگ‌های قدیمی
    /// </summary>
    [HttpDelete("old/{daysToKeep}")]
    public async Task<IActionResult> DeleteOldLogs(int daysToKeep, CancellationToken cancellationToken)
    {
        var response = await _queryLogService.DeleteOldLogsAsync(daysToKeep, cancellationToken);
        return ToActionResult(response);
    }
}
