using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Chat;

/// <summary>
/// موجودیت پیام چت
/// این موجودیت پیام‌های چت بین کاربران و پشتیبان را نگهداری می‌کند
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// متن پیام
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// شناسه کاربر فرستنده (null برای پیام‌های پشتیبان)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// شناسه پشتیبان فرستنده (null برای پیام‌های کاربر)
    /// </summary>
    public int? SupportUserId { get; set; }

    /// <summary>
    /// شماره موبایل کاربر (Foreign Key به ChatSession)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// ارتباط با جلسه چت
    /// </summary>
    public virtual ChatSession? ChatSession { get; set; }

    /// <summary>
    /// آیا پیام خوانده شده است
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// نوع فرستنده (User یا Support)
    /// </summary>
    public string SenderType { get; set; } = "User"; // "User" or "Support"

    /// <summary>
    /// نام فرستنده (برای نمایش)
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// آیا این پیام از طرف پشتیبان است
    /// </summary>
    public bool IsFromSupport => SupportUserId.HasValue;
}
