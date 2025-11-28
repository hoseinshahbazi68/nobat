using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Nobat.Domain.Common;
using Nobat.Domain.Interfaces;
using Nobat.Infrastructure.Data;

namespace Nobat.Infrastructure.Repositories;

/// <summary>
/// کلاس پایه ریپازیتوری
/// </summary>
/// <typeparam name="T">نوع موجودیت</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// کانتکست پایگاه داده
    /// </summary>
    protected readonly ApplicationDbContext _context;

    /// <summary>
    /// مجموعه موجودیت‌ها
    /// </summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// سازنده ریپازیتوری
    /// </summary>
    /// <param name="context">کانتکست پایگاه داده</param>
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// دریافت موجودیت بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه موجودیت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>موجودیت یافت شده یا null</returns>
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// دریافت تمام موجودیت‌ها
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مجموعه موجودیت‌ها</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// جستجوی موجودیت‌ها بر اساس شرط
    /// </summary>
    /// <param name="predicate">شرط جستجو</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مجموعه موجودیت‌های یافت شده</returns>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// افزودن موجودیت جدید
    /// </summary>
    /// <param name="entity">موجودیت برای افزودن</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>موجودیت افزوده شده</returns>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// بروزرسانی موجودیت
    /// </summary>
    /// <param name="entity">موجودیت برای بروزرسانی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// حذف موجودیت بر اساس شناسه
    /// </summary>
    /// <param name="id">شناسه موجودیت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    /// <summary>
    /// بررسی وجود موجودیت بر اساس شرط
    /// </summary>
    /// <param name="predicate">شرط بررسی</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>true اگر موجودیت وجود داشته باشد، در غیر این صورت false</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// دریافت کوئری قابل استفاده برای فیلتر و مرتب‌سازی
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>کوئری قابل استفاده</returns>
    public virtual Task<IQueryable<T>> GetQueryableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dbSet.AsQueryable());
    }

    /// <summary>
    /// دریافت موجودیت بر اساس شناسه (بدون tracking - برای read-only)
    /// </summary>
    /// <param name="id">شناسه موجودیت</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>موجودیت یافت شده یا null</returns>
    public virtual async Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// دریافت تمام موجودیت‌ها (بدون tracking - برای read-only)
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مجموعه موجودیت‌ها</returns>
    public virtual async Task<IEnumerable<T>> GetAllNoTrackingAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// جستجوی موجودیت‌ها بر اساس شرط (بدون tracking - برای read-only)
    /// </summary>
    /// <param name="predicate">شرط جستجو</param>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>مجموعه موجودیت‌های یافت شده</returns>
    public virtual async Task<IEnumerable<T>> FindNoTrackingAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// دریافت کوئری قابل استفاده برای فیلتر و مرتب‌سازی (بدون tracking - برای read-only)
    /// </summary>
    /// <param name="cancellationToken">توکن لغو عملیات</param>
    /// <returns>کوئری قابل استفاده بدون tracking</returns>
    public virtual Task<IQueryable<T>> GetQueryableNoTrackingAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dbSet.AsNoTracking().AsQueryable());
    }
}
