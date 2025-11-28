namespace Nobat.Application.QueryLogs.Dto;

/// <summary>
/// DTO لاگ کوئری
/// </summary>
public class QueryLogDto
{
    /// <summary>
    /// شناسه لاگ
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// متن کوئری SQL
    /// </summary>
    public string QueryText { get; set; } = string.Empty;

    /// <summary>
    /// پارامترهای کوئری (JSON)
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// زمان اجرای کوئری به میلی‌ثانیه
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// زمان شروع اجرای کوئری
    /// </summary>
    public DateTime ExecutionTime { get; set; }

    /// <summary>
    /// نوع دستور (SELECT, INSERT, UPDATE, DELETE, etc.)
    /// </summary>
    public string? CommandType { get; set; }

    /// <summary>
    /// نام جداول استفاده شده در کوئری
    /// </summary>
    public string? TablesUsed { get; set; }

    /// <summary>
    /// آدرس IP کاربر
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// نام Controller و Action که کوئری را فراخوانی کرده
    /// </summary>
    public string? ControllerAction { get; set; }

    /// <summary>
    /// آیا کوئری سنگین است
    /// </summary>
    public bool IsHeavy { get; set; }

    /// <summary>
    /// خطا در صورت وجود
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
