using Nobat.Domain.Enums;

namespace Nobat.Application.Users.Dto;

/// <summary>
/// DTO کاربر
/// </summary>
public class UserDto
{
    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام کاربری
    /// </summary>
    public string NationalCode { get; set; } = string.Empty;

    /// <summary>
    /// ایمیل
    /// </summary>
    public string Email { get; set; } = string.Empty;

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
    /// جنسیت
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// تاریخ تولد
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// نام شهر
    /// </summary>
    public string? CityName { get; set; }

    /// <summary>
    /// نام استان
    /// </summary>
    public string? ProvinceName { get; set; }

    /// <summary>
    /// مسیر عکس پروفایل
    /// </summary>
    public string? ProfilePicture { get; set; }

    /// <summary>
    /// وضعیت فعال بودن
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// لیست نقش‌ها
    /// </summary>
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// DTO ایجاد کاربر
/// </summary>
public class CreateUserDto
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
    /// جنسیت
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// تاریخ تولد
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// لیست شناسه نقش‌ها
    /// </summary>
    public List<int> RoleIds { get; set; } = new();
}

/// <summary>
/// DTO به‌روزرسانی کاربر
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام کاربری
    /// </summary>
    public string NationalCode { get; set; } = string.Empty;

    /// <summary>
    /// ایمیل
    /// </summary>
    public string Email { get; set; } = string.Empty;

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
    /// جنسیت
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// تاریخ تولد
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// وضعیت فعال بودن
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// لیست شناسه نقش‌ها
    /// </summary>
    public List<int> RoleIds { get; set; } = new();
}

/// <summary>
/// DTO به‌روزرسانی پروفایل
/// </summary>
public class UpdateProfileDto
{
    /// <summary>
    /// ایمیل
    /// </summary>
    public string Email { get; set; } = string.Empty;

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
    /// جنسیت
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// تاریخ تولد
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// مسیر عکس پروفایل
    /// </summary>
    public string? ProfilePicture { get; set; }
}

    /// <summary>
    /// DTO تغییر رمز عبور
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// رمز عبور فعلی
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// رمز عبور جدید
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO اختصاص کلینیک به کاربر
    /// </summary>
    public class AssignClinicToUserDto
    {
        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int ClinicId { get; set; }
    }
