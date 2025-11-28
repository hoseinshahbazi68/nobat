namespace Nobat.Application.Chat.Dto;

/// <summary>
/// DTO پیام چت
/// </summary>
public class ChatMessageDto
{
    /// <summary>
    /// شناسه پیام
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// متن پیام
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// شناسه کاربر فرستنده
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// شناسه پشتیبان فرستنده
    /// </summary>
    public int? SupportUserId { get; set; }

    /// <summary>
    /// شماره موبایل کاربر
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// شناسه جلسه چت
    /// </summary>
    public string ChatSessionId { get; set; } = string.Empty;

    /// <summary>
    /// آیا پیام خوانده شده است
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// نوع فرستنده
    /// </summary>
    public string SenderType { get; set; } = "User";

    /// <summary>
    /// نام فرستنده
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// نام کامل پشتیبان (در صورت پاسخ از پشتیبان)
    /// </summary>
    public string? SupportUserName { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد پیام چت
/// </summary>
public class CreateChatMessageDto
{
    /// <summary>
    /// متن پیام
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// شماره موبایل کاربر
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// DTO پاسخ پشتیبان
/// </summary>
public class SupportReplyDto
{
    /// <summary>
    /// متن پاسخ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// شماره موبایل کاربر
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}
