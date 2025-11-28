using Microsoft.AspNetCore.Mvc;
using Nobat.Application.Common;

namespace Nobat.API.Controllers;

/// <summary>
/// کنترلر پایه برای تمام کنترلرها
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// دریافت شناسه کاربر فعلی
    /// </summary>
    /// <returns>شناسه کاربر یا 0 در صورت عدم احراز هویت</returns>
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    /// <summary>
    /// دریافت نام کاربری فعلی
    /// </summary>
    /// <returns>نام کاربری یا null</returns>
    protected string? GetCurrentUsername()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// دریافت نقش‌های کاربر فعلی
    /// </summary>
    /// <returns>مجموعه نقش‌ها</returns>
    protected IEnumerable<string> GetCurrentUserRoles()
    {
        return User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
    }

    /// <summary>
    /// تبدیل ApiResponse به ActionResult
    /// </summary>
    protected IActionResult ToActionResult<T>(ApiResponse<T> response)
    {
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// تبدیل ApiResponse به ActionResult (بدون داده)
    /// </summary>
    protected IActionResult ToActionResult(ApiResponse response)
    {
        return StatusCode(response.StatusCode, response);
    }
}
