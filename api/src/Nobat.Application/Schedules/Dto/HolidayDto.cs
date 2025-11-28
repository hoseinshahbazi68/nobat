namespace Nobat.Application.Schedules.Dto;

/// <summary>
/// DTO روز تعطیل
/// </summary>
public class HolidayDto
{
    /// <summary>
    /// شناسه تعطیل
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// تاریخ تعطیل
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// نام تعطیل
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
/// DTO ایجاد روز تعطیل
/// </summary>
public class CreateHolidayDto
{
    /// <summary>
    /// تاریخ تعطیل
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// نام تعطیل
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی روز تعطیل
/// </summary>
public class UpdateHolidayDto
{
    /// <summary>
    /// شناسه تعطیل
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// تاریخ تعطیل
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// نام تعطیل
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}
