namespace Nobat.Domain.Interfaces;

/// <summary>
/// رابط واحد کار برای مدیریت تراکنش‌ها
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// ذخیره تغییرات در پایگاه داده
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>تعداد ردیف‌های تأثیر یافته</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// شروع تراکنش جدید
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// تأیید و ذخیره تراکنش
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// بازگشت تراکنش
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

