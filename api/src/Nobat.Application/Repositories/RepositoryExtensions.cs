using Microsoft.EntityFrameworkCore;
using Nobat.Domain.Entities.Chat;
using Nobat.Domain.Entities.Users;
using Nobat.Domain.Interfaces;

namespace Nobat.Application.Repositories;

/// <summary>
/// Extension methods برای Repository های خاص در لایه Application
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// دریافت کاربر بر اساس کد ملی
    /// </summary>
    public static async Task<User?> GetByNationalCodeAsync(this IRepository<User> repository, string nationalCode, CancellationToken cancellationToken = default)
    {
        var queryable = await repository.GetQueryableAsync(cancellationToken);
        return await queryable
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.City)
                .ThenInclude(c => c.Province)
            .FirstOrDefaultAsync(u => u.NationalCode == nationalCode, cancellationToken);
    }

    /// <summary>
    /// دریافت کاربر بر اساس نام کاربری (برای backward compatibility)
    /// </summary>
    [Obsolete("Use GetByNationalCodeAsync instead")]
    public static async Task<User?> GetByUsernameAsync(this IRepository<User> repository, string username, CancellationToken cancellationToken = default)
    {
        return await repository.GetByNationalCodeAsync(username, cancellationToken);
    }

    /// <summary>
    /// دریافت کاربر بر اساس ایمیل
    /// </summary>
    public static async Task<User?> GetByEmailAsync(this IRepository<User> repository, string email, CancellationToken cancellationToken = default)
    {
        var queryable = await repository.GetQueryableAsync(cancellationToken);
        return await queryable
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.City)
                .ThenInclude(c => c.Province)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// دریافت کاربر به همراه نقش‌هایش
    /// </summary>
    public static async Task<User?> GetWithRolesAsync(this IRepository<User> repository, int id, CancellationToken cancellationToken = default)
    {
        var queryable = await repository.GetQueryableAsync(cancellationToken);
        return await queryable
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.City)
                .ThenInclude(c => c.Province)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// دریافت کاربر بر اساس شماره موبایل
    /// </summary>
    public static async Task<User?> GetByPhoneNumberAsync(this IRepository<User> repository, string phoneNumber, CancellationToken cancellationToken = default)
    {
        var queryable = await repository.GetQueryableAsync(cancellationToken);
        return await queryable
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
    }

    /// <summary>
    /// دریافت نقش بر اساس نام
    /// </summary>
    public static async Task<Role?> GetByNameAsync(this IRepository<Role> repository, string name, CancellationToken cancellationToken = default)
    {
        var queryable = await repository.GetQueryableAsync(cancellationToken);
        return await queryable
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    /// <summary>
    /// دریافت جلسه چت بر اساس شماره موبایل
    /// </summary>
    public static async Task<ChatSession?> GetByPhoneNumberAsync(this IRepository<ChatSession> repository, string phoneNumber, CancellationToken cancellationToken = default)
    {
        var queryable = await repository.GetQueryableAsync(cancellationToken);
        return await queryable
            .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber, cancellationToken);
    }
}
