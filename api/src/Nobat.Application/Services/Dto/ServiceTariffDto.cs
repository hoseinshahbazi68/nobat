namespace Nobat.Application.Services.Dto;

/// <summary>
/// DTO تعرفه خدمت
/// </summary>
public class ServiceTariffDto
{
    /// <summary>
    /// شناسه تعرفه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// نام خدمت
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// شناسه بیمه
    /// </summary>
    public int InsuranceId { get; set; }

    /// <summary>
    /// نام بیمه
    /// </summary>
    public string? InsuranceName { get; set; }

    /// <summary>
    /// شناسه پزشک (اختیاری)
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// نام پزشک
    /// </summary>
    public string? DoctorName { get; set; }

    /// <summary>
    /// شناسه کلینیک
    /// </summary>
    public int ClinicId { get; set; }

    /// <summary>
    /// نام کلینیک
    /// </summary>
    public string? ClinicName { get; set; }

    /// <summary>
    /// قیمت
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// قیمت نهایی
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// مدت زمان ویزیت (به دقیقه)
    /// </summary>
    public int? VisitDuration { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد تعرفه خدمت
/// </summary>
public class CreateServiceTariffDto
{
    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// شناسه بیمه
    /// </summary>
    public int InsuranceId { get; set; }

    /// <summary>
    /// شناسه پزشک (اختیاری)
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// شناسه کلینیک
    /// </summary>
    public int ClinicId { get; set; }

    /// <summary>
    /// قیمت
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// مدت زمان ویزیت (به دقیقه)
    /// </summary>
    public int? VisitDuration { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی تعرفه خدمت
/// </summary>
public class UpdateServiceTariffDto
{
    /// <summary>
    /// شناسه تعرفه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// شناسه بیمه
    /// </summary>
    public int InsuranceId { get; set; }

    /// <summary>
    /// شناسه پزشک (اختیاری)
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// شناسه کلینیک
    /// </summary>
    public int ClinicId { get; set; }

    /// <summary>
    /// قیمت
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// مدت زمان ویزیت (به دقیقه)
    /// </summary>
    public int? VisitDuration { get; set; }
}
