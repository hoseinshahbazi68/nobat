namespace Nobat.Application.Doctors.Dto;

/// <summary>
/// DTO تخصص
/// </summary>
public class SpecialtyDto
{
    /// <summary>
    /// شناسه تخصص
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام تخصص
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات تخصص
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد تخصص
/// </summary>
public class CreateSpecialtyDto
{
    /// <summary>
    /// نام تخصص
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات تخصص
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی تخصص
/// </summary>
public class UpdateSpecialtyDto
{
    /// <summary>
    /// شناسه تخصص
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام تخصص
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات تخصص
    /// </summary>
    public string? Description { get; set; }
}
