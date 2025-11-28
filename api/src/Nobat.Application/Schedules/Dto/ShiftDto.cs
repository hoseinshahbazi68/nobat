namespace Nobat.Application.Schedules.Dto;

/// <summary>
/// DTO شیفت
/// </summary>
public class ShiftDto
{
    /// <summary>
    /// شناسه شیفت
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام شیفت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// زمان شروع شیفت
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// زمان پایان شیفت
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات شیفت
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد شیفت
/// </summary>
public class CreateShiftDto
{
    /// <summary>
    /// نام شیفت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// زمان شروع شیفت
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// زمان پایان شیفت
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات شیفت
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی شیفت
/// </summary>
public class UpdateShiftDto
{
    /// <summary>
    /// شناسه شیفت
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام شیفت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// زمان شروع شیفت
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// زمان پایان شیفت
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات شیفت
    /// </summary>
    public string? Description { get; set; }
}
