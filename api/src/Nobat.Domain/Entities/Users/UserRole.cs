using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Users;

/// <summary>
/// موجودیت ارتباط کاربر و نقش
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// شناسه نقش
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// کاربر مرتبط
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// نقش مرتبط
    /// </summary>
    public virtual Role Role { get; set; } = null!;
}
