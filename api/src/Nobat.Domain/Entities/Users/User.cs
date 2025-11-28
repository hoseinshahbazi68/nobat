using Nobat.Domain.Common;
using Nobat.Domain.Entities.Clinics;
using Nobat.Domain.Entities.Locations;
using Nobat.Domain.Enums;

namespace Nobat.Domain.Entities.Users;

/// <summary>
/// موجودیت کاربر
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// کد ملی
    /// </summary>
    public string NationalCode { get; set; } = string.Empty;

    /// <summary>
    /// ایمیل کاربر
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// هش رمز عبور
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

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
    /// شهر
    /// </summary>
    public virtual City? City { get; set; }

    /// <summary>
    /// نشان می‌دهد که آیا کاربر فعال است یا خیر
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// مجموعه نقش‌های کاربر
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// مجموعه کلینیک‌هایی که کاربر مدیریت می‌کند (many-to-many)
    /// </summary>
    public virtual ICollection<ClinicUser> ClinicUsers { get; set; } = new List<ClinicUser>();

    /// <summary>
    /// مسیر عکس پروفایل
    /// </summary>
    public string? ProfilePicture { get; set; }
}
