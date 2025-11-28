using Nobat.Application.Chat.Dto;
using Nobat.Application.Common;

namespace Nobat.Application.Chat;

/// <summary>
/// رابط سرویس چت
/// </summary>
public interface IChatService
{
    /// <summary>
    /// ارسال پیام از کاربر
    /// </summary>
    Task<ApiResponse<ChatMessageDto>> SendUserMessageAsync(CreateChatMessageDto dto, int? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// ارسال پاسخ از پشتیبان
    /// </summary>
    Task<ApiResponse<ChatMessageDto>> SendSupportReplyAsync(SupportReplyDto dto, int supportUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت پیام‌های یک جلسه چت
    /// </summary>
    Task<ApiResponse<List<ChatMessageDto>>> GetChatMessagesAsync(string chatSessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت پیام‌های یک شماره موبایل
    /// </summary>
    Task<ApiResponse<List<ChatMessageDto>>> GetChatMessagesByPhoneAsync(string phoneNumber, int? supportUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت لیست جلسه‌های چت فعال (برای پشتیبان)
    /// </summary>
    Task<ApiResponse<List<ChatSessionDto>>> GetActiveChatSessionsAsync(string? phoneNumber = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف پیام‌های قدیمی‌تر از 3 روز
    /// </summary>
    Task<ApiResponse<int>> DeleteExpiredMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// علامت‌گذاری پیام‌ها به عنوان خوانده شده
    /// </summary>
    Task<ApiResponse> MarkMessagesAsReadAsync(string chatSessionId, int? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت تعداد پیام‌های پاسخ داده نشده (برای پشتیبان)
    /// </summary>
    Task<ApiResponse<int>> GetUnansweredMessagesCountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO جلسه چت
/// </summary>
public class ChatSessionDto
{
    public string ChatSessionId { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? UserName { get; set; }
    public int? UserId { get; set; }
    public int UnreadCount { get; set; }
    public DateTime LastMessageTime { get; set; }
    public string? LastMessage { get; set; }
}
