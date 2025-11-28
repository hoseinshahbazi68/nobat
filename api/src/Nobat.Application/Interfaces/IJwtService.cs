using Nobat.Domain.Entities.Users;

namespace Nobat.Application.Jwt;

/// <summary>
/// رابط سرویس JWT
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// تولید توکن JWT برای کاربر
    /// </summary>
    /// <param name="user">کاربر</param>
    /// <param name="roles">نقش‌های کاربر</param>
    /// <returns>توکن JWT</returns>
    string GenerateToken(User user, IEnumerable<string> roles);

    /// <summary>
    /// اعتبارسنجی توکن JWT
    /// </summary>
    /// <param name="token">توکن برای اعتبارسنجی</param>
    /// <returns>ClaimsPrincipal در صورت معتبر بودن، در غیر این صورت null</returns>
    System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);
}
