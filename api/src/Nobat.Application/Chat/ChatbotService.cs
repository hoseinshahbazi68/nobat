using Microsoft.Extensions.Logging;

namespace Nobat.Application.Chat;

/// <summary>
/// سرویس چت‌بات هوشمند
/// این سرویس به سوالات متداول کاربران به صورت خودکار پاسخ می‌دهد
/// </summary>
public class ChatbotService : IChatbotService
{
    private readonly ILogger<ChatbotService> _logger;

    // کلیدواژه‌های مربوط به درخواست ارتباط با کارشناس
    private readonly string[] _supportRequestKeywords = new[]
    {
        "کارشناس", "پشتیبان", "مشاور", "مشورت", "صحبت", "تماس", "ارتباط",
        "می‌خوام با کارشناس", "می‌خواهم با کارشناس", "می‌خوام با پشتیبان",
        "می‌خواهم با پشتیبان", "می‌خوام با مشاور", "می‌خواهم با مشاور",
        "با کارشناس صحبت", "با پشتیبان صحبت", "با مشاور صحبت",
        "کارشناس می‌خوام", "پشتیبان می‌خوام", "مشاور می‌خوام"
    };

    // سوالات متداول و پاسخ‌های آن‌ها
    private readonly Dictionary<string[], string> _faqResponses = new()
    {
        // سوالات مربوط به ساعت کاری
        {
            new[] { "ساعت", "ساعت کاری", "ساعت کار", "زمان", "چه ساعتی", "کی" },
            "ساعت کاری ما از شنبه تا پنج‌شنبه از 8 صبح تا 5 عصر است. در صورت نیاز می‌توانید با کارشناس ما تماس بگیرید."
        },
        // سوالات مربوط به آدرس
        {
            new[] { "آدرس", "کجاست", "کجا", "مکان", "موقعیت" },
            "آدرس ما در تهران است. برای دریافت آدرس دقیق می‌توانید با کارشناس ما تماس بگیرید."
        },
        // سوالات مربوط به تماس
        {
            new[] { "تلفن", "شماره", "تماس", "زنگ", "تلفن بزنم" },
            "شماره تماس ما در صفحه تماس با ما موجود است. همچنین می‌توانید از طریق همین چت با کارشناس ما در ارتباط باشید."
        },
        // سوالات مربوط به نوبت
        {
            new[] { "نوبت", "رزرو", "رزرو نوبت", "نوبت بگیرم", "وقت" },
            "برای رزرو نوبت می‌توانید از طریق سایت اقدام کنید یا با کارشناس ما تماس بگیرید."
        },
        // سلام و احوال‌پرسی
        {
            new[] { "سلام", "درود", "صبح بخیر", "عصر بخیر", "خوبی", "چطوری" },
            "سلام! خوش آمدید. چطور می‌توانم کمکتان کنم؟"
        },
        // تشکر
        {
            new[] { "ممنون", "متشکرم", "مرسی", "تشکر", "خدا قوت" },
            "خواهش می‌کنم. اگر سوال دیگری دارید، بپرسید."
        },
        // خداحافظی
        {
            new[] { "خداحافظ", "بای", "خداحافظی", "خداحفظ" },
            "خداحافظ! موفق باشید."
        },
        // سوالات مربوط به قیمت
        {
            new[] { "قیمت", "هزینه", "چقدر", "مبلغ", "تعرفه" },
            "برای اطلاع از قیمت‌ها و تعرفه‌ها می‌توانید با کارشناس ما تماس بگیرید یا از صفحه تعرفه‌ها استفاده کنید."
        },
        // سوالات مربوط به خدمات
        {
            new[] { "خدمات", "چه خدماتی", "چه کاری", "چه کارهایی" },
            "ما خدمات مختلفی ارائه می‌دهیم. برای اطلاع از جزئیات می‌توانید با کارشناس ما تماس بگیرید."
        }
    };

    public ChatbotService(ILogger<ChatbotService> logger)
    {
        _logger = logger;
    }

    // کلیدواژه‌های پاسخ مثبت
    private readonly string[] _positiveResponseKeywords = new[]
    {
        "بله", "آره", "بلی", "باشه", "حتما", "موافقم", "خوب", "باش", "بله می‌خوام",
        "آره می‌خوام", "بله می‌خواهم", "آره می‌خواهم", "می‌خوام", "می‌خواهم"
    };

    // پیام سوال chatbot
    private const string SupportConnectionQuestion = "متأسفانه نتوانستم پاسخ سوال شما را بدهم. آیا می‌خواهید با کارشناس ما در ارتباط باشید؟";

    // متن سوالات متداول
    private const string FaqMessage = @"متأسفانه نتوانستم پاسخ سوال شما را بدهم.

سوالات متداول:
• ساعت کاری: شنبه تا پنج‌شنبه از 8 صبح تا 5 عصر
• رزرو نوبت: از طریق سایت یا تماس با کارشناس
• آدرس: در تهران - برای آدرس دقیق با کارشناس تماس بگیرید
• تماس: از طریق همین چت یا صفحه تماس با ما
• قیمت و تعرفه: با کارشناس تماس بگیرید یا از صفحه تعرفه‌ها استفاده کنید

آیا می‌خواهید با کارشناس ما در ارتباط باشید؟";

    /// <summary>
    /// بررسی اینکه آیا می‌توان به صورت خودکار به پیام کاربر پاسخ داد
    /// </summary>
    public string? GetAutoReply(string userMessage, string? lastBotMessage = null)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return null;

        var normalizedMessage = userMessage.Trim().ToLower();

        // اگر آخرین پیام chatbot سوال اتصال به پشتیبان بود و کاربر پاسخ مثبت داد
        if (!string.IsNullOrEmpty(lastBotMessage) &&
            lastBotMessage.Contains("آیا می‌خواهید با کارشناس") &&
            IsPositiveResponse(userMessage))
        {
            _logger.LogInformation("User confirmed to connect to support");
            return "پیام شما برای کارشناس ارسال شد. به زودی با شما تماس خواهیم گرفت.";
        }

        // بررسی سوالات متداول
        foreach (var faq in _faqResponses)
        {
            foreach (var keyword in faq.Key)
            {
                if (normalizedMessage.Contains(keyword.ToLower()))
                {
                    _logger.LogInformation("Auto-reply sent for keyword: {Keyword}", keyword);
                    return faq.Value;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// بررسی اینکه آیا پیام، پاسخ مثبت به سوال chatbot است
    /// </summary>
    public bool IsPositiveResponse(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return false;

        var normalizedMessage = userMessage.Trim().ToLower();

        foreach (var keyword in _positiveResponseKeywords)
        {
            if (normalizedMessage.Contains(keyword.ToLower()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// دریافت پیام سوال chatbot برای اتصال به پشتیبان
    /// </summary>
    public string GetSupportConnectionQuestion()
    {
        return SupportConnectionQuestion;
    }

    /// <summary>
    /// دریافت متن سوالات متداول همراه با سوال اتصال به کارشناس
    /// </summary>
    public string GetFaqMessageWithSupportQuestion()
    {
        return FaqMessage;
    }

    /// <summary>
    /// بررسی اینکه آیا کاربر می‌خواهد با کارشناس صحبت کند
    /// </summary>
    public bool WantsToTalkToSupport(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return false;

        var normalizedMessage = userMessage.Trim().ToLower();

        // بررسی کلیدواژه‌های مربوط به درخواست ارتباط با کارشناس
        foreach (var keyword in _supportRequestKeywords)
        {
            if (normalizedMessage.Contains(keyword.ToLower()))
            {
                _logger.LogInformation("User wants to talk to support. Message: {Message}", userMessage);
                return true;
            }
        }

        return false;
    }
}
