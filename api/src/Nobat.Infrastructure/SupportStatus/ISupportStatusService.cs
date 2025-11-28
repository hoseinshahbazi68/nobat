namespace Nobat.Infrastructure.SupportStatus;

/// <summary>
/// سرویس مدیریت وضعیت آنلاین پشتیبان‌ها
/// </summary>
public interface ISupportStatusService
{
    /// <summary>
    /// ثبت وضعیت آنلاین پشتیبان
    /// </summary>
    Task SetSupportOnlineAsync(int supportUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ثبت وضعیت آفلاین پشتیبان
    /// </summary>
    Task SetSupportOfflineAsync(int supportUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// بررسی اینکه آیا حداقل یک پشتیبان آنلاین است
    /// </summary>
    Task<bool> IsAnySupportOnlineAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت تعداد پشتیبان‌های آنلاین
    /// </summary>
    Task<int> GetOnlineSupportCountAsync(CancellationToken cancellationToken = default);
}
