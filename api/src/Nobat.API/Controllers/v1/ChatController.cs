using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nobat.API.Controllers;
using Nobat.Application.Chat;
using Nobat.Application.Chat.Dto;
using Nobat.Application.Common;
using Nobat.Infrastructure.SupportStatus;

namespace Nobat.API.Controllers.v1;

/// <summary>
/// کنترلر چت
/// این کنترلر API endpoints مربوط به چت را ارائه می‌دهد
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ChatController : BaseController
{
    private readonly IChatService _chatService;
    private readonly ISupportStatusService _supportStatusService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        ISupportStatusService supportStatusService,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _supportStatusService = supportStatusService;
        _logger = logger;
    }

    /// <summary>
    /// ارسال پیام از کاربر (بدون نیاز به لاگین)
    /// </summary>
    [HttpPost("send")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] CreateChatMessageDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(); // اگر لاگین نبود، 0 برمی‌گرداند
        var response = await _chatService.SendUserMessageAsync(dto, userId > 0 ? userId : null, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// ارسال پاسخ از پشتیبان
    /// فقط کاربرانی که نقش Support دارند می‌توانند پاسخ بدهند
    /// </summary>
    [HttpPost("reply")]
    [Authorize(Roles = "Admin,Support")]
    public async Task<IActionResult> SendReply([FromBody] SupportReplyDto dto, CancellationToken cancellationToken)
    {
        var supportUserId = GetCurrentUserId();
        if (supportUserId == 0)
        {
            return Unauthorized();
        }

        // بررسی اینکه کاربر نقش Support دارد
        var userRoles = GetCurrentUserRoles().ToList();
        if (!userRoles.Contains("Support") && !userRoles.Contains("Admin"))
        {
            return Forbid("فقط کاربران با نقش Support می‌توانند پاسخ بدهند");
        }

        // ثبت وضعیت آنلاین پشتیبان
        await _supportStatusService.SetSupportOnlineAsync(supportUserId, cancellationToken);

        var response = await _chatService.SendSupportReplyAsync(dto, supportUserId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت پیام‌های یک جلسه چت (بدون نیاز به لاگین)
    /// </summary>
    [HttpGet("messages/{chatSessionId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMessages(string chatSessionId, CancellationToken cancellationToken)
    {
        var response = await _chatService.GetChatMessagesAsync(chatSessionId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت پیام‌های یک شماره موبایل (بدون نیاز به لاگین)
    /// </summary>
    [HttpGet("messages/phone/{phoneNumber}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMessagesByPhone(string phoneNumber, CancellationToken cancellationToken)
    {
        var response = await _chatService.GetChatMessagesByPhoneAsync(phoneNumber, null, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت پیام‌های یک شماره موبایل برای پشتیبان (با ارسال پیام خوش‌آمدگویی در صورت نیاز)
    /// </summary>
    [HttpGet("messages/phone/{phoneNumber}/support")]
    [Authorize(Roles = "Admin,Support")]
    public async Task<IActionResult> GetMessagesByPhoneForSupport(string phoneNumber, CancellationToken cancellationToken)
    {
        var supportUserId = GetCurrentUserId();
        if (supportUserId == 0)
        {
            return Unauthorized();
        }

        // بررسی اینکه کاربر نقش Support دارد
        var userRoles = GetCurrentUserRoles().ToList();
        if (!userRoles.Contains("Support") && !userRoles.Contains("Admin"))
        {
            return Forbid("فقط کاربران با نقش Support می‌توانند به پیام‌ها دسترسی داشته باشند");
        }

        // ثبت وضعیت آنلاین پشتیبان
        await _supportStatusService.SetSupportOnlineAsync(supportUserId, cancellationToken);

        var response = await _chatService.GetChatMessagesByPhoneAsync(phoneNumber, supportUserId, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت لیست جلسه‌های چت فعال (فقط برای پشتیبان)
    /// فقط کاربرانی که نقش Support دارند می‌توانند به جلسه‌ها دسترسی داشته باشند
    /// </summary>
    [HttpGet("sessions")]
    [Authorize(Roles = "Admin,Support")]
    public async Task<IActionResult> GetActiveSessions([FromQuery] string? phoneNumber = null, CancellationToken cancellationToken = default)
    {
        var supportUserId = GetCurrentUserId();
        if (supportUserId == 0)
        {
            return Unauthorized();
        }

        // بررسی اینکه کاربر نقش Support دارد
        var userRoles = GetCurrentUserRoles().ToList();
        if (!userRoles.Contains("Support") && !userRoles.Contains("Admin"))
        {
            return Forbid("فقط کاربران با نقش Support می‌توانند به جلسه‌های چت دسترسی داشته باشند");
        }

        // ثبت وضعیت آنلاین پشتیبان
        await _supportStatusService.SetSupportOnlineAsync(supportUserId, cancellationToken);

        var response = await _chatService.GetActiveChatSessionsAsync(phoneNumber, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// بررسی وضعیت آنلاین پشتیبان‌ها (بدون نیاز به لاگین)
    /// </summary>
    [HttpGet("support-status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSupportStatus(CancellationToken cancellationToken = default)
    {
        var isOnline = await _supportStatusService.IsAnySupportOnlineAsync(cancellationToken);
        var onlineCount = await _supportStatusService.GetOnlineSupportCountAsync(cancellationToken);

        return Ok(new { isOnline, onlineCount });
    }

    /// <summary>
    /// حذف پیام‌های قدیمی‌تر از 3 روز
    /// </summary>
    [HttpPost("cleanup")]
   // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupExpiredMessages(CancellationToken cancellationToken = default)
    {
        var response = await _chatService.DeleteExpiredMessagesAsync(cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// علامت‌گذاری پیام‌ها به عنوان خوانده شده (بدون نیاز به لاگین)
    /// </summary>
    [HttpPost("mark-read/{chatSessionId}")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkAsRead(string chatSessionId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var response = await _chatService.MarkMessagesAsReadAsync(chatSessionId, userId > 0 ? userId : null, cancellationToken);
        return ToActionResult(response);
    }

    /// <summary>
    /// دریافت تعداد پیام‌های پاسخ داده نشده (فقط برای پشتیبان)
    /// فقط کاربرانی که نقش Support دارند می‌توانند این اطلاعات را دریافت کنند
    /// </summary>
    [HttpGet("unanswered-count")]
    [Authorize(Roles = "Admin,Support")]
    public async Task<IActionResult> GetUnansweredCount(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        // بررسی اینکه کاربر نقش Support دارد
        var userRoles = GetCurrentUserRoles().ToList();
        if (!userRoles.Contains("Support") && !userRoles.Contains("Admin"))
        {
            return Forbid("فقط کاربران با نقش Support می‌توانند تعداد پیام‌های پاسخ داده نشده را دریافت کنند");
        }

        var response = await _chatService.GetUnansweredMessagesCountAsync(cancellationToken);
        return ToActionResult(response);
    }
}
