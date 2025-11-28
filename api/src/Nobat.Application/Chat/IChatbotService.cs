namespace Nobat.Application.Chat;

/// <summary>
/// رابط سرویس چت‌بات هوشمند
/// این سرویس مسئول پاسخ خودکار به سوالات متداول کاربران است
/// </summary>
public interface IChatbotService
{
    /// <summary>
    /// بررسی اینکه آیا می‌توان به صورت خودکار به پیام کاربر پاسخ داد
    /// </summary>
    /// <param name="userMessage">پیام کاربر</param>
    /// <param name="lastBotMessage">آخرین پیام chatbot (برای بررسی سوال قبلی)</param>
    /// <returns>اگر بتوان پاسخ داد، پاسخ خودکار را برمی‌گرداند. در غیر این صورت null</returns>
    string? GetAutoReply(string userMessage, string? lastBotMessage = null);

    /// <summary>
    /// بررسی اینکه آیا کاربر می‌خواهد با کارشناس صحبت کند
    /// </summary>
    /// <param name="userMessage">پیام کاربر</param>
    /// <returns>true اگر کاربر می‌خواهد با کارشناس صحبت کند</returns>
    bool WantsToTalkToSupport(string userMessage);

    /// <summary>
    /// بررسی اینکه آیا پیام، پاسخ مثبت به سوال chatbot است
    /// </summary>
    /// <param name="userMessage">پیام کاربر</param>
    /// <returns>true اگر کاربر پاسخ مثبت داده است</returns>
    bool IsPositiveResponse(string userMessage);

    /// <summary>
    /// دریافت پیام سوال chatbot برای اتصال به پشتیبان
    /// </summary>
    /// <returns>پیام سوال</returns>
    string GetSupportConnectionQuestion();

    /// <summary>
    /// دریافت متن سوالات متداول همراه با سوال اتصال به کارشناس
    /// </summary>
    /// <returns>متن سوالات متداول</returns>
    string GetFaqMessageWithSupportQuestion();
}
