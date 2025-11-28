using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.DatabaseChangeLogs;
using Sieve.Models;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر مدیریت لاگ تغییرات دیتابیس
/// این کنترلر API endpoints مربوط به مشاهده لاگ تغییرات دیتابیس را ارائه می‌دهد
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "Admin")]
public class DatabaseChangeLogsController : BaseController
{
    private readonly IDatabaseChangeLogService _databaseChangeLogService;
    private readonly ILogger<DatabaseChangeLogsController> _logger;

    /// <summary>
    /// سازنده کنترلر لاگ تغییرات دیتابیس
    /// </summary>
    public DatabaseChangeLogsController(IDatabaseChangeLogService databaseChangeLogService, ILogger<DatabaseChangeLogsController> logger)
    {
        _databaseChangeLogService = databaseChangeLogService;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لاگ تغییرات بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _databaseChangeLogService.GetByIdAsync(id, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست تمام لاگ تغییرات
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _databaseChangeLogService.GetAllAsync(sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لاگ تغییرات بر اساس نام جدول
    /// </summary>
    [HttpGet("table/{tableName}")]
    public async Task<IActionResult> GetByTableName(string tableName, [FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _databaseChangeLogService.GetByTableNameAsync(tableName, sieveModel, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لاگ تغییرات بر اساس نوع تغییر
    /// </summary>
    [HttpGet("type/{changeType}")]
    public async Task<IActionResult> GetByChangeType(string changeType, [FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var response = await _databaseChangeLogService.GetByChangeTypeAsync(changeType, sieveModel, cancellationToken);
        return ToActionResult(response);
    }
}
