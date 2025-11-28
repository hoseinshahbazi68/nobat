using Nobat.Domain.Common;
using Nobat.Domain.Entities.Users;

namespace Nobat.Domain.Entities.Chat;

/// <summary>
/// موجودیت جلسه چت
/// این موجودیت اطلاعات جلسه‌های چت را نگهداری می‌کند
/// </summary>
public class ChatSession : BaseEntity
{
    /// <summary>
    /// شماره موبایل کاربر (کلید اصلی)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// شناسه کاربر (null برای کاربران مهمان)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// نام کاربر (برای نمایش سریع)
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// ارتباط با کاربر (اختیاری)
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// مجموعه پیام‌های این جلسه چت
    /// </summary>
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
