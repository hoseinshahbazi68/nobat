namespace Nobat.Application.Locations.Dto;

/// <summary>
/// DTO شهر
/// </summary>
public class CityDto
{
    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام شهر
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// شناسه استان
    /// </summary>
    public int ProvinceId { get; set; }

    /// <summary>
    /// نام استان
    /// </summary>
    public string? ProvinceName { get; set; }

    /// <summary>
    /// کد شهر
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد شهر
/// </summary>
public class CreateCityDto
{
    /// <summary>
    /// نام شهر
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// شناسه استان
    /// </summary>
    public int ProvinceId { get; set; }

    /// <summary>
    /// کد شهر
    /// </summary>
    public string? Code { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی شهر
/// </summary>
public class UpdateCityDto
{
    /// <summary>
    /// شناسه شهر
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام شهر
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// شناسه استان
    /// </summary>
    public int ProvinceId { get; set; }

    /// <summary>
    /// کد شهر
    /// </summary>
    public string? Code { get; set; }
}
