using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobat.Application.Chat.Dto;
using Nobat.Application.Common;
using Nobat.Application.Repositories;
using Nobat.Domain.Entities.Chat;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;

namespace Nobat.Application.Chat;

/// <summary>
/// سرویس چت
/// </summary>
public class ChatService : IChatService
{
    private readonly IRepository<ChatMessage> _chatRepository;
    private readonly IRepository<ChatSession> _chatSessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatbotService _chatbotService;

    public ChatService(
        IRepository<ChatMessage> chatRepository,
        IRepository<ChatSession> chatSessionRepository,
        IRepository<User> userRepository,
        IMapper mapper,
        ILogger<ChatService> logger,
        IUnitOfWork unitOfWork,
        IChatbotService chatbotService)
    {
        _chatRepository = chatRepository;
        _chatSessionRepository = chatSessionRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _chatbotService = chatbotService;
    }

    public async Task<ApiResponse<ChatMessageDto>> SendUserMessageAsync(CreateChatMessageDto dto, int? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // اعتبارسنجی ورودی
            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return ApiResponse<ChatMessageDto>.Error("متن پیام نمی‌تواند خالی باشد", 400);
            }

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                return ApiResponse<ChatMessageDto>.Error("شماره موبایل نمی‌تواند خالی باشد", 400);
            }

            string? senderName = "کاربر مهمان";
            string phoneNumber = dto.PhoneNumber.Trim();

            // اگر کاربر لاگین بود، اطلاعاتش را دریافت می‌کنیم
            if (userId.HasValue && userId.Value > 0)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
                if (user != null)
                {
                    senderName = $"{user.FirstName} {user.LastName}";
                    // اگر شماره موبایل از کاربر گرفته نشد، از دیتابیس می‌گیریم
                    if (string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        phoneNumber = user.PhoneNumber;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(phoneNumber))
            {
                // اگر کاربر لاگین نبود، بر اساس شماره موبایل جستجو می‌کنیم
                var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
                if (user != null)
                {
                    senderName = $"{user.FirstName} {user.LastName}";
                    userId = user.Id;
                }
            }

            // بررسی نهایی که phoneNumber خالی نباشد
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return ApiResponse<ChatMessageDto>.Error("شماره موبایل نمی‌تواند خالی باشد", 400);
            }

            // ایجاد یا به‌روزرسانی ChatSession
            var chatSession = await _chatSessionRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
            if (chatSession == null)
            {
                chatSession = new ChatSession
                {
                    PhoneNumber = phoneNumber,
                    UserId = userId,
                    UserName = senderName
                };
                await _chatSessionRepository.AddAsync(chatSession, cancellationToken);
            }
            else
            {
                // به‌روزرسانی اطلاعات جلسه در صورت تغییر
                if (chatSession.UserId != userId)
                {
                    chatSession.UserId = userId;
                }
                if (chatSession.UserName != senderName)
                {
                    chatSession.UserName = senderName;
                }
                await _chatSessionRepository.UpdateAsync(chatSession, cancellationToken);
            }

            // بررسی اینکه آیا قبلاً در این جلسه پیامی برای کارشناس ارسال شده است
            // اگر بله، دیگر chatbot بررسی نمی‌شود و تمام پیام‌ها مستقیماً برای کارشناس ارسال می‌شوند
            var query = await _chatRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var expirationDate = DateTime.UtcNow.AddDays(-3);

            // بررسی اینکه آیا پیام کاربری وجود دارد که برای کارشناس ارسال شده (خوانده نشده)
            // یا پیام پشتیبانی وجود دارد (یعنی کارشناس قبلاً پاسخ داده)
            var hasSupportConnection = await query
                .AnyAsync(m => m.PhoneNumber == phoneNumber
                    && m.CreatedAt >= expirationDate
                    && ((m.SenderType == "User" && !m.IsRead) // پیام کاربری که خوانده نشده (برای کارشناس ارسال شده)
                        || (m.SenderType == "Support" && m.SenderName != "سیستم هوشمند")), // پیام پشتیبانی (کارشناس قبلاً پاسخ داده)
                    cancellationToken);

            // اگر قبلاً به کارشناس وصل شده است، تمام پیام‌ها مستقیماً برای کارشناس ارسال می‌شوند
            if (hasSupportConnection)
            {
                _logger.LogInformation("User already connected to support. PhoneNumber: {PhoneNumber}. Saving message directly to support.", phoneNumber);

                var chatMessagesend = new ChatMessage
                {
                    Message = dto.Message,
                    UserId = userId,
                    PhoneNumber = phoneNumber,
                    SenderType = "User",
                    SenderName = senderName,
                    IsRead = false
                };

                await _chatRepository.AddAsync(chatMessagesend, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var messageDtoret = _mapper.Map<ChatMessageDto>(chatMessagesend);
                return ApiResponse<ChatMessageDto>.Success(messageDtoret, "پیام شما برای کارشناس ارسال شد", 201);
            }

            // بررسی اینکه آیا کاربر می‌خواهد با کارشناس صحبت کند (مستقیم)
            bool shouldForwardToSupport = _chatbotService.WantsToTalkToSupport(dto.Message);

            // اگر کاربر نمی‌خواهد با کارشناس صحبت کند، سعی می‌کنیم به صورت خودکار پاسخ دهیم
            if (!shouldForwardToSupport)
            {
                // بررسی پاسخ خودکار
                var autoReply = _chatbotService.GetAutoReply(dto.Message, null);
                if (!string.IsNullOrEmpty(autoReply))
                {
                    // ذخیره پیام کاربر در دیتابیس
                    var userMessage = new ChatMessage
                    {
                        Message = dto.Message,
                        UserId = userId,
                        PhoneNumber = phoneNumber,
                        SenderType = "User",
                        SenderName = senderName,
                        IsRead = false
                    };
                    await _chatRepository.AddAsync(userMessage, cancellationToken);

                    // ذخیره پاسخ chatbot در دیتابیس
                    var botMessage = new ChatMessage
                    {
                        Message = autoReply,
                        PhoneNumber = phoneNumber,
                        SenderType = "Support",
                        SenderName = "سیستم هوشمند",
                        IsRead = true
                    };
                    await _chatRepository.AddAsync(botMessage, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Auto-reply sent and saved to database. PhoneNumber: {PhoneNumber}", phoneNumber);

                    // برگرداندن پاسخ chatbot
                    var autoReplyDto = _mapper.Map<ChatMessageDto>(botMessage);
                    return ApiResponse<ChatMessageDto>.Success(autoReplyDto, "پاسخ خودکار ارسال شد", 201);
                }
                else
                {
                    // اگر نتوانستیم پاسخ دهیم، بررسی می‌کنیم که آیا کاربر پاسخ مثبت به سوال قبلی chatbot داده است
                    // (مثلاً "بله" به سوال "آیا می‌خواهید با کارشناس ما در ارتباط باشید؟")
                    bool isPositiveResponse = _chatbotService.IsPositiveResponse(dto.Message);

                    if (isPositiveResponse)
                    {
                        // کاربر پاسخ مثبت داده است، پیامش را ذخیره می‌کنیم
                        _logger.LogInformation("User confirmed to connect to support. PhoneNumber: {PhoneNumber}. Saving message to database.", phoneNumber);

                        var chatMessage1 = new ChatMessage
                        {
                            Message = dto.Message,
                            UserId = userId,
                            PhoneNumber = phoneNumber,
                            SenderType = "User",
                            SenderName = senderName,
                            IsRead = false
                        };

                        await _chatRepository.AddAsync(chatMessage1, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                          var messageDtmop= _mapper.Map<ChatMessageDto>(chatMessage1);
                        return ApiResponse<ChatMessageDto>.Success(messageDtmop, "پیام شما برای کارشناس ارسال شد", 201);
                    }
                    else
                    {
                        // اگر پاسخ مثبت نداد، متن سوالات متداول را می‌فرستیم و ذخیره می‌کنیم
                        // ذخیره پیام کاربر در دیتابیس
                        var userMessage = new ChatMessage
                        {
                            Message = dto.Message,
                            UserId = userId,
                            PhoneNumber = phoneNumber,
                            SenderType = "User",
                            SenderName = senderName,
                            IsRead = false
                        };
                        await _chatRepository.AddAsync(userMessage, cancellationToken);

                        // ذخیره پیام FAQ chatbot در دیتابیس
                        var faqMessage = new ChatMessage
                        {
                            Message = _chatbotService.GetFaqMessageWithSupportQuestion(),
                            PhoneNumber = phoneNumber,
                            SenderType = "Support",
                            SenderName = "سیستم هوشمند",
                            IsRead = true
                        };
                        await _chatRepository.AddAsync(faqMessage, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Chatbot FAQ message sent and saved to database. PhoneNumber: {PhoneNumber}", phoneNumber);

                        // برگرداندن متن سوالات متداول
                        var faqMessageDto = _mapper.Map<ChatMessageDto>(faqMessage);
                        return ApiResponse<ChatMessageDto>.Success(faqMessageDto, "متن سوالات متداول ارسال شد", 201);
                    }
                }
            }

            // اگر کاربر خواست با کارشناس صحبت کند، پیام کاربر را ذخیره می‌کنیم
            _logger.LogInformation("User requested to talk to support. PhoneNumber: {PhoneNumber}. Saving message to database.", phoneNumber);

            var chatMessage = new ChatMessage
            {
                Message = dto.Message,
                UserId = userId,
                PhoneNumber = phoneNumber,
                SenderType = "User",
                SenderName = senderName,
                IsRead = false
            };

            await _chatRepository.AddAsync(chatMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageDto = _mapper.Map<ChatMessageDto>(chatMessage);
            return ApiResponse<ChatMessageDto>.Success(messageDto, "پیام شما برای کارشناس ارسال شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending user message");
            return ApiResponse<ChatMessageDto>.Error("خطا در ارسال پیام", ex);
        }
    }

    public async Task<ApiResponse<ChatMessageDto>> SendSupportReplyAsync(SupportReplyDto dto, int supportUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var supportUser = await _userRepository.GetByIdAsync(supportUserId, cancellationToken);
            if (supportUser == null)
            {
                return ApiResponse<ChatMessageDto>.Error("کاربر پشتیبان یافت نشد", 404);
            }

            var chatMessage = new ChatMessage
            {
                Message = dto.Message,
                SupportUserId = supportUserId,
                PhoneNumber = dto.PhoneNumber,
                SenderType = "Support",
                SenderName = $"{supportUser.FirstName} {supportUser.LastName}",
                IsRead = false
            };

            await _chatRepository.AddAsync(chatMessage, cancellationToken);

            // علامت‌گذاری پیام‌های کاربر در این جلسه به عنوان خوانده شده
            await MarkMessagesAsReadAsync(dto.PhoneNumber, null, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Support reply sent. PhoneNumber: {PhoneNumber}, SupportUserId: {SupportUserId}", dto.PhoneNumber, supportUserId);
            var messageDto = _mapper.Map<ChatMessageDto>(chatMessage);
            // تنظیم SupportUserName به صورت دستی برای اطمینان
            messageDto.SupportUserName = chatMessage.SenderName;
            return ApiResponse<ChatMessageDto>.Success(messageDto, "پاسخ با موفقیت ارسال شد", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending support reply");
            return ApiResponse<ChatMessageDto>.Error("خطا در ارسال پاسخ", ex);
        }
    }

    public async Task<ApiResponse<List<ChatMessageDto>>> GetChatMessagesAsync(string chatSessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            // chatSessionId در واقع PhoneNumber است
            string phoneNumber = chatSessionId;
            if (chatSessionId.StartsWith("chat-"))
            {
                phoneNumber = chatSessionId.Substring(5); // حذف "chat-" از ابتدا
            }

            var query = await _chatRepository.GetQueryableNoTrackingAsync(cancellationToken);

            // فیلتر پیام‌های قدیمی‌تر از 3 روز را حذف می‌کنیم
            var expirationDate = DateTime.UtcNow.AddDays(-3);

            // فیلتر پیام‌های chatbot (پیام‌های chatbot نباید در دیتابیس ذخیره شوند)
            var messages = await query
                .Where(m => m.PhoneNumber == phoneNumber
                    && m.CreatedAt >= expirationDate) // همه پیام‌ها (شامل chatbot) را نمایش می‌دهیم
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);

            var messagesDto = _mapper.Map<List<ChatMessageDto>>(messages);
            // تنظیم SupportUserName برای هر پیام پشتیبان
            foreach (var msgDto in messagesDto)
            {
                if (msgDto.SenderType == "Support" && !string.IsNullOrEmpty(msgDto.SenderName) && msgDto.SenderName != "سیستم هوشمند")
                {
                    msgDto.SupportUserName = msgDto.SenderName;
                }
            }
            return ApiResponse<List<ChatMessageDto>>.Success(messagesDto, "پیام‌ها با موفقیت دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat messages");
            return ApiResponse<List<ChatMessageDto>>.Error("خطا در دریافت پیام‌ها", ex);
        }
    }

    public async Task<ApiResponse<List<ChatMessageDto>>> GetChatMessagesByPhoneAsync(string phoneNumber, int? supportUserId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _chatRepository.GetQueryableAsync(cancellationToken);

            // فیلتر پیام‌های قدیمی‌تر از 3 روز را حذف می‌کنیم
            var expirationDate = DateTime.UtcNow.AddDays(-3);

            // اگر پشتیبان چت را باز کرده است، بررسی می‌کنیم که آیا پیام خوش‌آمدگویی ارسال شده یا نه
            if (supportUserId.HasValue && supportUserId.Value > 0)
            {
                var supportUser = await _userRepository.GetByIdAsync(supportUserId.Value, cancellationToken);
                if (supportUser != null)
                {
                    // بررسی اینکه آیا این پشتیبان قبلاً به این جلسه پاسخ داده است
                    var hasPreviousSupportMessage = await query
                        .AnyAsync(m => m.PhoneNumber == phoneNumber
                            && m.SupportUserId == supportUserId.Value
                            && m.SenderType == "Support"
                            && m.SenderName != "سیستم هوشمند", cancellationToken);

                    // اگر اولین بار است که این پشتیبان این جلسه را باز می‌کند، پیام خوش‌آمدگویی را ارسال می‌کنیم
                    if (!hasPreviousSupportMessage)
                    {
                        var welcomeMessage = new ChatMessage
                        {
                            Message = "سلام وقت بخیر! چطوری می‌تونم کمکتون کنم؟",
                            SupportUserId = supportUserId.Value,
                            PhoneNumber = phoneNumber,
                            SenderType = "Support",
                            SenderName = $"{supportUser.FirstName} {supportUser.LastName}",
                            IsRead = false
                        };

                        await _chatRepository.AddAsync(welcomeMessage, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Welcome message sent when support opened chat. PhoneNumber: {PhoneNumber}, SupportUserId: {SupportUserId}", phoneNumber, supportUserId.Value);
                    }
                }
            }

            // فیلتر پیام‌های chatbot (پیام‌های chatbot نباید در دیتابیس ذخیره شوند)
            var messages = await query
                .Where(m => m.PhoneNumber == phoneNumber
                    && m.CreatedAt >= expirationDate) // همه پیام‌ها (شامل chatbot) را نمایش می‌دهیم
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);

            var messagesDto = _mapper.Map<List<ChatMessageDto>>(messages);
            // تنظیم SupportUserName برای هر پیام پشتیبان
            foreach (var msgDto in messagesDto)
            {
                if (msgDto.SenderType == "Support" && !string.IsNullOrEmpty(msgDto.SenderName) && msgDto.SenderName != "سیستم هوشمند")
                {
                    msgDto.SupportUserName = msgDto.SenderName;
                }
            }
            return ApiResponse<List<ChatMessageDto>>.Success(messagesDto, "پیام‌ها با موفقیت دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat messages by phone");
            return ApiResponse<List<ChatMessageDto>>.Error("خطا در دریافت پیام‌ها", ex);
        }
    }

    public async Task<ApiResponse<List<ChatSessionDto>>> GetActiveChatSessionsAsync(string? phoneNumber = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionQuery = await _chatSessionRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var messageQuery = await _chatRepository.GetQueryableNoTrackingAsync(cancellationToken);

            // فیلتر بر اساس PhoneNumber اگر مشخص شده باشد
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                sessionQuery = sessionQuery.Where(s => s.PhoneNumber == phoneNumber);
            }

            // دریافت جلسه‌های چت با Join به پیام‌ها
            var sessions = await sessionQuery
                .Select(s => new
                {
                    Session = s,
                    LastMessage = messageQuery
                        .Where(m => m.PhoneNumber == s.PhoneNumber) // همه پیام‌ها (شامل chatbot) را نمایش می‌دهیم
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),
                    UnreadCount = messageQuery
                        .Count(m => m.PhoneNumber == s.PhoneNumber && !m.IsRead && m.SenderType == "User")
                })
                .ToListAsync(cancellationToken);

            var sessionDtos = new List<ChatSessionDto>();

            foreach (var item in sessions)
            {
                var session = item.Session;
                var userName = session.UserName;

                // اگر UserName در جلسه نیست، از User بگیریم
                if (string.IsNullOrEmpty(userName) && session.UserId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(session.UserId.Value, cancellationToken);
                    if (user != null)
                    {
                        userName = $"{user.FirstName} {user.LastName}";
                    }
                }

                // اگر هنوز UserName نداریم، از شماره موبایل استفاده کنیم
                if (string.IsNullOrEmpty(userName))
                {
                    userName = $"کاربر {session.PhoneNumber}";
                }

                sessionDtos.Add(new ChatSessionDto
                {
                    ChatSessionId = $"chat-{session.PhoneNumber}", // برای سازگاری با کد موجود
                    PhoneNumber = session.PhoneNumber,
                    UserId = session.UserId,
                    UserName = userName,
                    UnreadCount = item.UnreadCount,
                    LastMessageTime = item.LastMessage?.CreatedAt ?? session.CreatedAt,
                    LastMessage = item.LastMessage?.Message
                });
            }

            // مرتب‌سازی بر اساس آخرین پیام
            sessionDtos = sessionDtos.OrderByDescending(s => s.LastMessageTime).ToList();

            return ApiResponse<List<ChatSessionDto>>.Success(sessionDtos, "جلسه‌های چت با موفقیت دریافت شدند");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active chat sessions");
            return ApiResponse<List<ChatSessionDto>>.Error("خطا در دریافت جلسه‌های چت", ex);
        }
    }

    public async Task<ApiResponse> MarkMessagesAsReadAsync(string chatSessionId, int? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // chatSessionId در واقع PhoneNumber است
            string phoneNumber = chatSessionId;
            if (chatSessionId.StartsWith("chat-"))
            {
                phoneNumber = chatSessionId.Substring(5); // حذف "chat-" از ابتدا
            }

            var query = await _chatRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var messages = await query
                .Where(m => m.PhoneNumber == phoneNumber && !m.IsRead)
                .ToListAsync(cancellationToken);

            // اگر userId مشخص شده باشد، فقط پیام‌های کاربر را علامت‌گذاری می‌کنیم
            // در غیر این صورت همه پیام‌ها را علامت‌گذاری می‌کنیم
            if (userId.HasValue)
            {
                messages = messages.Where(m => m.UserId == userId).ToList();
            }

            foreach (var message in messages)
            {
                message.IsRead = true;
                await _chatRepository.UpdateAsync(message, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Messages marked as read. PhoneNumber: {PhoneNumber}", phoneNumber);
            return ApiResponse.Success("پیام‌ها به عنوان خوانده شده علامت‌گذاری شدند");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read");
            return ApiResponse.Error("خطا در علامت‌گذاری پیام‌ها", ex);
        }
    }

    public async Task<ApiResponse<int>> DeleteExpiredMessagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _chatRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var expirationDate = DateTime.UtcNow.AddDays(-3);

            // دریافت پیام‌های قدیمی‌تر از 3 روز
            var expiredMessages = await query
                .Where(m => m.CreatedAt < expirationDate)
                .ToListAsync(cancellationToken);

            int deletedCount = 0;
            foreach (var message in expiredMessages)
            {
                await _chatRepository.DeleteAsync(message.Id, cancellationToken);
                deletedCount++;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted {Count} expired chat messages", deletedCount);
            return ApiResponse<int>.Success(deletedCount, $"{deletedCount} پیام قدیمی حذف شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expired messages");
            return ApiResponse<int>.Error("خطا در حذف پیام‌های قدیمی", ex);
        }
    }

    public async Task<ApiResponse<int>> GetUnansweredMessagesCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionQuery = await _chatSessionRepository.GetQueryableNoTrackingAsync(cancellationToken);
            var messageQuery = await _chatRepository.GetQueryableNoTrackingAsync(cancellationToken);

            // فیلتر پیام‌های قدیمی‌تر از 3 روز را حذف می‌کنیم
            var expirationDate = DateTime.UtcNow.AddDays(-3);

            // دریافت جلسه‌های چت با Join به پیام‌ها
            var sessions = await sessionQuery
                .Select(s => new
                {
                    PhoneNumber = s.PhoneNumber,
                    LastMessage = messageQuery
                        .Where(m => m.PhoneNumber == s.PhoneNumber
                            && m.CreatedAt >= expirationDate) // همه پیام‌ها (شامل chatbot) را نمایش می‌دهیم
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),
                    LastUserMessage = messageQuery
                        .Where(m => m.PhoneNumber == s.PhoneNumber
                            && m.CreatedAt >= expirationDate
                            && m.SenderType == "User")
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),
                    LastSupportMessage = messageQuery
                        .Where(m => m.PhoneNumber == s.PhoneNumber
                            && m.CreatedAt >= expirationDate
                            && m.SenderType == "Support")
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            // شمارش جلسه‌هایی که پاسخ داده نشده‌اند
            // یک جلسه پاسخ داده نشده است اگر:
            // 1. آخرین پیام از طرف کاربر است
            // 2. یا پیام کاربری وجود دارد که بعد از آخرین پیام پشتیبان است
            var unansweredCount = 0;
            foreach (var session in sessions)
            {
                if (session.LastMessage == null) continue;

                // اگر آخرین پیام از طرف کاربر است
                if (session.LastMessage.SenderType == "User")
                {
                    unansweredCount++;
                }
                // اگر پیام کاربری وجود دارد که بعد از آخرین پیام پشتیبان است
                else if (session.LastUserMessage != null && session.LastSupportMessage != null)
                {
                    if (session.LastUserMessage.CreatedAt > session.LastSupportMessage.CreatedAt)
                    {
                        unansweredCount++;
                    }
                }
                // اگر پیام کاربری وجود دارد اما پیام پشتیبانی وجود ندارد
                else if (session.LastUserMessage != null && session.LastSupportMessage == null)
                {
                    unansweredCount++;
                }
            }

            _logger.LogInformation("Found {Count} unanswered chat sessions from database", unansweredCount);
            return ApiResponse<int>.Success(unansweredCount, "تعداد جلسه‌های چت پاسخ داده نشده از دیتابیس دریافت شد");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unanswered messages count from database");
            return ApiResponse<int>.Error("خطا در دریافت تعداد جلسه‌های چت پاسخ داده نشده از دیتابیس", ex);
        }
    }
}
