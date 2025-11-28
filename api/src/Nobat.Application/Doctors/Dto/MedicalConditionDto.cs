namespace Nobat.Application.Doctors.Dto;

/// <summary>
/// DTO علائم و مشکلات پزشکی
/// </summary>
public class MedicalConditionDto
{
    /// <summary>
    /// شناسه علائم
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام علائم یا مشکل
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات علائم
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد علائم پزشکی
/// </summary>
public class CreateMedicalConditionDto
{
    /// <summary>
    /// نام علائم یا مشکل
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات علائم
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی علائم پزشکی
/// </summary>
public class UpdateMedicalConditionDto
{
    /// <summary>
    /// شناسه علائم
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام علائم یا مشکل
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات علائم
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO رابطه پزشک و علائم پزشکی
/// </summary>
public class DoctorMedicalConditionDto
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
    /// شناسه علائم پزشکی
    /// </summary>
    public int MedicalConditionId { get; set; }

    /// <summary>
    /// اطلاعات علائم پزشکی
    /// </summary>
    public MedicalConditionDto? MedicalCondition { get; set; }
}

/// <summary>
/// DTO رابطه تخصص و علائم پزشکی
/// </summary>
public class SpecialtyMedicalConditionDto
{
    /// <summary>
    /// شناسه رابطه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه تخصص
    /// </summary>
    public int SpecialtyId { get; set; }

    /// <summary>
    /// شناسه علائم پزشکی
    /// </summary>
    public int MedicalConditionId { get; set; }

    /// <summary>
    /// اطلاعات علائم پزشکی
    /// </summary>
    public MedicalConditionDto? MedicalCondition { get; set; }
}
