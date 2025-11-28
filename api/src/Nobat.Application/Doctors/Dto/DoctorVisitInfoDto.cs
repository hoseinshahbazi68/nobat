namespace Nobat.Application.Doctors.Dto;

/// <summary>
/// DTO اطلاعات ویزیت پزشک
/// </summary>
public class DoctorVisitInfoDto
{
    /// <summary>
    /// شناسه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// نام پزشک
    /// </summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>
    /// کد نظام پزشکی
    /// </summary>
    public string MedicalCode { get; set; } = string.Empty;

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
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاریخ به‌روزرسانی
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد اطلاعات ویزیت پزشک
/// </summary>
public class CreateDoctorVisitInfoDto
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

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
}

/// <summary>
/// DTO به‌روزرسانی اطلاعات ویزیت پزشک
/// </summary>
public class UpdateDoctorVisitInfoDto
{
    /// <summary>
    /// شناسه
    /// </summary>
    public int Id { get; set; }

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
}
