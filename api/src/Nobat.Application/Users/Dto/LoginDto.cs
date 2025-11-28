using Nobat.Domain.Enums;

namespace Nobat.Application.Users.Dto;

/// <summary>
/// DTO ورود کاربر
/// </summary>
public class LoginDto
{
    /// <summary>
    /// نام کاربری
    /// </summary>
    public string NationalCode { get; set; } = string.Empty;

    /// <summary>
    /// رمز عبور
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO ثبت‌نام کاربر
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// نام کاربری
    /// </summary>
    public string NationalCode { get; set; } = string.Empty;

    /// <summary>
    /// ایمیل
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// رمز عبور
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// نام
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// نام خانوادگی
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// شماره تلفن
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// جنسیت
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// تاریخ تولد
    /// </summary>
    public DateTime? BirthDate { get; set; }
}

/// <summary>
/// DTO پاسخ احراز هویت
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// توکن دسترسی
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// اطلاعات کاربر
    /// </summary>
    public UserDto User { get; set; } = null!;

    /// <summary>
    /// تاریخ انقضای توکن
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
