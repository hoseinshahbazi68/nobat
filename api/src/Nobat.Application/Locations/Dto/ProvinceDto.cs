namespace Nobat.Application.Locations.Dto;

/// <summary>
/// DTO استان
/// </summary>
public class ProvinceDto
{
    /// <summary>
    /// شناسه استان
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام استان
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// کد استان
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد استان
/// </summary>
public class CreateProvinceDto
{
    /// <summary>
    /// نام استان
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// کد استان
    /// </summary>
    public string? Code { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی استان
/// </summary>
public class UpdateProvinceDto
{
    /// <summary>
    /// شناسه استان
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام استان
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// کد استان
    /// </summary>
    public string? Code { get; set; }
}
