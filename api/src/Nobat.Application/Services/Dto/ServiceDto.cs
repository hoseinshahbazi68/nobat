namespace Nobat.Application.Services.Dto;

/// <summary>
/// DTO خدمت
/// </summary>
public class ServiceDto
{
    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام خدمت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد خدمت
/// </summary>
public class CreateServiceDto
{
    /// <summary>
    /// نام خدمت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی خدمت
/// </summary>
public class UpdateServiceDto
{
    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام خدمت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}
