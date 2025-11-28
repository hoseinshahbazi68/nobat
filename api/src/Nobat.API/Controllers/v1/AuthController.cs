using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Common;
using Nobat.Application.Users;
using Nobat.Application.Users.Dto;
using Nobat.Infrastructure.SupportStatus;
using Nobat.Application.Clinics.Dto;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر احراز هویت
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : BaseController
{
    /// <summary>
    /// سرویس کاربر
    /// </summary>
    private readonly IUserService _userService;

    /// <summary>
    /// سرویس مدیریت وضعیت پشتیبان
    /// </summary>
    private readonly ISupportStatusService _supportStatusService;

    /// <summary>
    /// سرویس آپلود فایل
    /// </summary>
    private readonly IFileUploadService _fileUploadService;

    /// <summary>
    /// لاگر
    /// </summary>
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// سازنده کنترلر احراز هویت
    /// </summary>
    /// <param name="userService">سرویس کاربر</param>
    /// <param name="supportStatusService">سرویس مدیریت وضعیت پشتیبان</param>
    /// <param name="fileUploadService">سرویس آپلود فایل</param>
    /// <param name="logger">لاگر</param>
    public AuthController(
        IUserService userService,
        ISupportStatusService supportStatusService,
        IFileUploadService fileUploadService,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _supportStatusService = supportStatusService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// ورود کاربر
    /// </summary>
    /// <param name="loginDto">اطلاعات ورود</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ احراز هویت</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        var response = await _userService.LoginAsync(loginDto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ثبت‌نام کاربر جدید
    /// </summary>
    /// <param name="registerDto">اطلاعات ثبت‌نام</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ احراز هویت</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
    {
        var response = await _userService.RegisterAsync(registerDto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت اطلاعات کاربر فعلی
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>اطلاعات کاربر</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            var errorResponse = ApiResponse<UserDto>.Error("کاربر احراز هویت نشده است", 401);
            return ToActionResult(errorResponse);
        }

        var response = await _userService.GetCurrentUserAsync(userId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// به‌روزرسانی پروفایل کاربر
    /// </summary>
    /// <param name="updateDto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>اطلاعات کاربر به‌روز شده</returns>
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateDto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            var errorResponse = ApiResponse<UserDto>.Error("کاربر احراز هویت نشده است", 401);
            return ToActionResult(errorResponse);
        }

        var response = await _userService.UpdateProfileAsync(userId, updateDto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// تغییر رمز عبور کاربر
    /// </summary>
    /// <param name="changePasswordDto">اطلاعات تغییر رمز عبور</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            var errorResponse = ApiResponse.Error("کاربر احراز هویت نشده است", 401);
            return ToActionResult(errorResponse);
        }

        var response = await _userService.ChangePasswordAsync(userId, changePasswordDto, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست کلینیک‌های کاربر فعلی
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>لیست کلینیک‌های کاربر</returns>
    [HttpGet("me/clinics")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserClinics(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            var errorResponse = ApiResponse<List<ClinicDto>>.Error("کاربر احراز هویت نشده است", 401);
            return ToActionResult(errorResponse);
        }

        var response = await _userService.GetUserClinicsAsync(userId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// آپلود عکس پروفایل کاربر
    /// </summary>
    /// <param name="file">فایل عکس</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>اطلاعات کاربر به‌روز شده</returns>
    [HttpPost("me/profile-picture")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                var errorResponse = ApiResponse<UserDto>.Error("کاربر احراز هویت نشده است", 401);
                return ToActionResult(errorResponse);
            }

            // آپلود فایل
            var profilePicturePath = await _fileUploadService.UploadProfilePictureAsync(file, userId, cancellationToken);

            if (string.IsNullOrEmpty(profilePicturePath))
            {
                return ToActionResult(ApiResponse<UserDto>.Error("خطا در آپلود عکس پروفایل", 400));
            }

            // به‌روزرسانی مسیر عکس در پروفایل کاربر
            var updateDto = new UpdateProfileDto
            {
                ProfilePicture = profilePicturePath
            };

            var response = await _userService.UpdateProfileAsync(userId, updateDto, cancellationToken);
            return ToActionResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture");
            return ToActionResult(ApiResponse<UserDto>.Error("خطا در آپلود عکس پروفایل", ex));
        }
    }

    /// <summary>
    /// خروج کاربر از سیستم
    /// اگر کاربر نقش Support دارد، وضعیت آنلاینش از کش حذف می‌شود
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                var errorResponse = ApiResponse.Error("کاربر احراز هویت نشده است", 401);
                return ToActionResult(errorResponse);
            }

            // بررسی نقش‌های کاربر
            var userRoles = GetCurrentUserRoles().ToList();

            // اگر کاربر نقش Support دارد، وضعیت آنلاینش را از کش حذف می‌کنیم
            if (userRoles.Contains("Support") || userRoles.Contains("Admin"))
            {
                await _supportStatusService.SetSupportOfflineAsync(userId, cancellationToken);
                _logger.LogInformation("Support user {UserId} logged out and marked as offline", userId);
            }

            return ToActionResult(ApiResponse.Success("خروج با موفقیت انجام شد"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return ToActionResult(ApiResponse.Error("خطا در فرآیند خروج", ex));
        }
    }
}
