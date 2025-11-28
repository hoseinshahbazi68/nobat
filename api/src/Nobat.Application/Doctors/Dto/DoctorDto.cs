using Microsoft.AspNetCore.Http;
using Nobat.Application.Clinics.Dto;
using Nobat.Domain.Enums;

namespace Nobat.Application.Doctors.Dto;

/// <summary>
/// DTO رابطه پزشک و تخصص
/// </summary>
public class DoctorSpecialtyDto
{
    /// <summary>
    /// شناسه رابطه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// شناسه تخصص
    /// </summary>
    public int SpecialtyId { get; set; }

    /// <summary>
    /// اطلاعات تخصص
    /// </summary>
    public SpecialtyDto? Specialty { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// DTO پزشک
/// این کلاس برای انتقال اطلاعات پزشک از API به کلاینت استفاده می‌شود
/// شامل تمام اطلاعات لازم برای نمایش و مدیریت پزشک
/// </summary>
public class DoctorDto
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int Id { get; set; }

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
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// ایمیل
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// کد نظام پزشکی
    /// </summary>
    public string MedicalCode { get; set; } = string.Empty;

    /// <summary>
    /// پیشوند پزشک
    /// </summary>
    public DoctorPrefix Prefix { get; set; } = DoctorPrefix.None;

    /// <summary>
    /// درجه علمی
    /// </summary>
    public ScientificDegree ScientificDegree { get; set; } = ScientificDegree.None;

    /// <summary>
    /// کد   ملی
    /// </summary>
    public string NationalCode { get; set; } = string.Empty;

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// لیست کلینیک‌های پزشک
    /// </summary>
    public List<ClinicDto> Clinics { get; set; } = new List<ClinicDto>();

    /// <summary>
    /// لیست تخصص‌های پزشک
    /// </summary>
    public List<DoctorSpecialtyDto> Specialties { get; set; } = new List<DoctorSpecialtyDto>();

    /// <summary>
    /// لیست علائم پزشکی مرتبط با پزشک
    /// </summary>
    public List<DoctorMedicalConditionDto> MedicalConditions { get; set; } = new List<DoctorMedicalConditionDto>();

    /// <summary>
    /// درباره پزشک
    /// </summary>
    public string? About { get; set; }

    /// <summary>
    /// آدرس مطب
    /// </summary>
    public string? ClinicAddress { get; set; }

    /// <summary>
    /// تلفن مطب
    /// </summary>
    public string? ClinicPhone { get; set; }

    /// <summary>
    /// ساعت حضور در مطب
    /// </summary>
    public string? OfficeHours { get; set; }

    /// <summary>
    /// شناسه شهر (برای به‌روزرسانی کاربر)
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// جنسیت (برای به‌روزرسانی کاربر)
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// تاریخ تولد (برای به‌روزرسانی کاربر)
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// مسیر عکس پروفایل
    /// </summary>
    public string? ProfilePicture { get; set; }
}

public class DoctorListDto
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int Id { get; set; }

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
    public string Phone { get; set; } = string.Empty;


    /// <summary>
    /// کد نظام پزشکی
    /// </summary>
    public string MedicalCode { get; set; } = string.Empty;

    /// <summary>
    /// پیشوند پزشک
    /// </summary>
    public DoctorPrefix Prefix { get; set; } = DoctorPrefix.None;

    /// <summary>
    /// درجه علمی
    /// </summary>
    public ScientificDegree ScientificDegree { get; set; } = ScientificDegree.None;

    /// <summary>
    /// کد   ملی
    /// </summary>
    public string NationalCode { get; set; } = string.Empty;

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}


/// <summary>
/// DTO ایجاد پزشک
/// این کلاس برای دریافت اطلاعات لازم برای ایجاد پزشک جدید از کلاینت استفاده می‌شود
/// </summary>
public class CreateDoctorDto
{
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
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// کد نظام پزشکی
    /// </summary>
    public string MedicalCode { get; set; } = string.Empty;

    /// <summary>
    /// پیشوند پزشک
    /// </summary>
    public DoctorPrefix Prefix { get; set; } = DoctorPrefix.None;

    /// <summary>
    /// درجه علمی
    /// </summary>
    public ScientificDegree ScientificDegree { get; set; } = ScientificDegree.None;

    /// <summary>
    /// شناسه کاربر (اختیاری)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// کد ملی کاربر (برای جستجو و ایجاد کاربر در صورت عدم وجود)
    /// </summary>
    public string? NationalCode { get; set; }

    /// <summary>
    /// شناسه کلینیک (اختیاری - برای افزودن پزشک به کلینیک)
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// لیست شناسه تخصص‌ها
    /// </summary>
    public List<int>? SpecialtyIds { get; set; }

    /// <summary>
    /// لیست شناسه علائم پزشکی
    /// </summary>
    public List<int>? MedicalConditionIds { get; set; }

    /// <summary>
    /// شناسه شهر (برای ایجاد کاربر جدید)
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// ایمیل (برای ایجاد کاربر جدید)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// جنسیت (برای ایجاد کاربر جدید)
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// تاریخ تولد (برای ایجاد کاربر جدید)
    /// </summary>
    public DateTime? BirthDate { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی پزشک
/// این کلاس برای دریافت اطلاعات لازم برای به‌روزرسانی پزشک از کلاینت استفاده می‌شود
/// باید شامل شناسه پزشک باشد
/// </summary>
public class UpdateDoctorDto
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int Id { get; set; }

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
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// ایمیل
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// کد نظام پزشکی
    /// </summary>
    public string MedicalCode { get; set; } = string.Empty;

    /// <summary>
    /// پیشوند پزشک
    /// </summary>
    public DoctorPrefix Prefix { get; set; } = DoctorPrefix.None;

    /// <summary>
    /// درجه علمی
    /// </summary>
    public ScientificDegree ScientificDegree { get; set; } = ScientificDegree.None;

    /// <summary>
    /// شناسه کاربر (اختیاری)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// شناسه کلینیک (اختیاری - برای افزودن پزشک به کلینیک)
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// لیست شناسه تخصص‌ها
    /// </summary>
    public List<int>? SpecialtyIds { get; set; }

    /// <summary>
    /// لیست شناسه علائم پزشکی
    /// </summary>
    public List<int>? MedicalConditionIds { get; set; }

    /// <summary>
    /// درباره پزشک
    /// </summary>
    public string? About { get; set; }

    /// <summary>
    /// آدرس مطب
    /// </summary>
    public string? ClinicAddress { get; set; }

    /// <summary>
    /// تلفن مطب
    /// </summary>
    public string? ClinicPhone { get; set; }

    /// <summary>
    /// ساعت حضور در مطب
    /// </summary>
    public string? OfficeHours { get; set; }

    /// <summary>
    /// شناسه شهر (برای به‌روزرسانی کاربر)
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// جنسیت (برای به‌روزرسانی کاربر)
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// تاریخ تولد (برای به‌روزرسانی کاربر)
    /// </summary>
    public DateTime? BirthDate { get; set; }
}

/// <summary>
/// DTO ایجاد پزشک با فایل
/// </summary>
public class CreateDoctorWithFileDto : CreateDoctorDto
{
    /// <summary>
    /// فایل عکس پروفایل
    /// </summary>
    public IFormFile? ProfilePictureFile { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی پزشک با فایل
/// </summary>
public class UpdateDoctorWithFileDto : UpdateDoctorDto
{
    /// <summary>
    /// فایل عکس پروفایل
    /// </summary>
    public IFormFile? ProfilePictureFile { get; set; }
}
