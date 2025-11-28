namespace Nobat.Application.Clinics.Dto;

/// <summary>
/// DTO کلینیک
/// این کلاس برای انتقال اطلاعات کلینیک از API به کلاینت استفاده می‌شود
/// </summary>
public class ClinicDto
{
    /// <summary>
    /// شناسه کلینیک
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام کلینیک
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// آدرس کلینیک
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// شماره تلفن کلینیک
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// ایمیل کلینیک
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// نام شهر
    /// </summary>
    public string? CityName { get; set; }

    /// <summary>
    /// وضعیت فعال بودن
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// تعداد روز تولید نوبت‌دهی
    /// </summary>
    public int? AppointmentGenerationDays { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد کلینیک
/// </summary>
public class CreateClinicDto
{
    /// <summary>
    /// نام کلینیک
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// آدرس کلینیک
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// شماره تلفن کلینیک
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// ایمیل کلینیک
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// وضعیت فعال بودن
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// تعداد روز تولید نوبت‌دهی
    /// </summary>
    public int? AppointmentGenerationDays { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی کلینیک
/// </summary>
public class UpdateClinicDto
{
    /// <summary>
    /// شناسه کلینیک
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام کلینیک
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// آدرس کلینیک
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// شماره تلفن کلینیک
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// ایمیل کلینیک
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// وضعیت فعال بودن
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// تعداد روز تولید نوبت‌دهی
    /// </summary>
    public int? AppointmentGenerationDays { get; set; }
}

/// <summary>
/// DTO ساده کلینیک (فقط نام و شناسه)
/// برای استفاده در dropdown و لیست‌های ساده
/// </summary>
public class ClinicSimpleDto
{
    /// <summary>
    /// شناسه کلینیک
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام کلینیک
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
