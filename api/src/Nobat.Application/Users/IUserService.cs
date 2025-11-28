using Nobat.Application.Common;
using Nobat.Application.Schedules;
using Nobat.Application.Users.Dto;
using Sieve.Models;

namespace Nobat.Application.Users;

/// <summary>
/// رابط سرویس کاربر
/// </summary>
public interface IUserService
{
    /// <summary>
    /// دریافت کاربر بر اساس شناسه
    /// </summary>
    Task<ApiResponse<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت کاربر بر اساس کد ملی
    /// </summary>
    /// <param name="nationalCode">کد ملی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>اطلاعات کاربر</returns>
    Task<ApiResponse<UserDto>> GetByNationalCodeAsync(string nationalCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست کاربران با فیلتر و مرتب‌سازی
    /// </summary>
    Task<ApiResponse<PagedResult<UserDto>>> GetAllAsync(SieveModel sieveModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// ایجاد کاربر جدید
    /// </summary>
    Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی کاربر
    /// </summary>
    Task<ApiResponse<UserDto>> UpdateAsync(UpdateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف کاربر
    /// </summary>
    Task<ApiResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// ورود کاربر
    /// </summary>
    /// <param name="loginDto">اطلاعات ورود</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ احراز هویت شامل توکن و اطلاعات کاربر</returns>
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// ثبت‌نام کاربر جدید
    /// </summary>
    /// <param name="registerDto">اطلاعات ثبت‌نام</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>پاسخ احراز هویت شامل توکن و اطلاعات کاربر</returns>
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت اطلاعات کاربر فعلی
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>اطلاعات کاربر</returns>
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// به‌روزرسانی پروفایل کاربر
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="updateDto">اطلاعات به‌روزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>اطلاعات کاربر به‌روز شده</returns>
    Task<ApiResponse<UserDto>> UpdateProfileAsync(int userId, UpdateProfileDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// تغییر رمز عبور کاربر
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="changePasswordDto">اطلاعات تغییر رمز عبور</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    Task<ApiResponse> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست کلینیک‌های کاربر
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>لیست کلینیک‌های کاربر</returns>
    Task<ApiResponse<List<Clinics.Dto.ClinicDto>>> GetUserClinicsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// اختصاص کلینیک به کاربر
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="dto">اطلاعات اختصاص کلینیک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    Task<ApiResponse> AssignClinicToUserAsync(int userId, AssignClinicToUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف دسترسی کاربر به کلینیک
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="clinicId">شناسه کلینیک</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    Task<ApiResponse> RemoveClinicFromUserAsync(int userId, int clinicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// تغییر رمز عبور کاربر توسط ادمین (بدون نیاز به رمز عبور فعلی)
    /// </summary>
    /// <param name="userId">شناسه کاربر</param>
    /// <param name="resetPasswordDto">اطلاعات تغییر رمز عبور</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>نتیجه عملیات</returns>
    Task<ApiResponse> ResetUserPasswordAsync(int userId, ResetUserPasswordDto resetPasswordDto, CancellationToken cancellationToken = default);
}
