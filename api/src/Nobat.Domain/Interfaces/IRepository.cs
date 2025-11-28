using System.Linq.Expressions;
using Nobat.Domain.Common;

namespace Nobat.Domain.Interfaces;

/// <summary>
/// رابط عمومی برای دسترسی به داده‌ها
/// </summary>
/// <typeparam name="T">نوع موجودیت</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// دریافت موجودیت بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه موجودیت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>موجودیت یافت شده یا null</returns>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت تمام موجودیت‌ها
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مجموعه موجودیت‌ها</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// جستجوی موجودیت‌ها بر اساس شرط
    /// </summary>
    /// <param name="predicate">شرط جستجو</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مجموعه موجودیت‌های یافت شده</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// افزودن موجودیت جدید
    /// </summary>
    /// <param name="entity">موجودیت برای افزودن</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>موجودیت افزوده شده</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// بروزرسانی موجودیت
    /// </summary>
    /// <param name="entity">موجودیت برای بروزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف موجودیت بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه موجودیت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// بررسی وجود موجودیت بر اساس شرط
    /// </summary>
    /// <param name="predicate">شرط بررسی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>true اگر موجودیت وجود داشته باشد، در غیر این صورت false</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت کوئری قابل استفاده برای فیلتر و مرتب‌سازی
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>کوئری قابل استفاده</returns>
    Task<IQueryable<T>> GetQueryableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت موجودیت بر اساس شناسه (بدون tracking - برای read-only)
    /// </summary>
    /// <param name="id">شناسه موجودیت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>موجودیت یافت شده یا null</returns>
    Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت تمام موجودیت‌ها (بدون tracking - برای read-only)
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مجموعه موجودیت‌ها</returns>
    Task<IEnumerable<T>> GetAllNoTrackingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// جستجوی موجودیت‌ها بر اساس شرط (بدون tracking - برای read-only)
    /// </summary>
    /// <param name="predicate">شرط جستجو</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مجموعه موجودیت‌های یافت شده</returns>
    Task<IEnumerable<T>> FindNoTrackingAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// دریافت کوئری قابل استفاده برای فیلتر و مرتب‌سازی (بدون tracking - برای read-only)
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>کوئری قابل استفاده بدون tracking</returns>
    Task<IQueryable<T>> GetQueryableNoTrackingAsync(CancellationToken cancellationToken = default);
}
