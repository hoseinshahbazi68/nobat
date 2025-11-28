using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Schedules;

/// <summary>
/// موجودیت شیفت
/// </summary>
public class Shift : BaseEntity
{
    /// <summary>
    /// نام شیفت
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// زمان شروع شیفت
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// زمان پایان شیفت
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// توضیحات شیفت
    /// </summary>
    public string? Description { get; set; }
}
