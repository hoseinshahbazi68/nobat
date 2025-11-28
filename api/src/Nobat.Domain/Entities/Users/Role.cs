using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Users;

/// <summary>
/// موجودیت نقش
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// نام نقش
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات نقش
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// مجموعه کاربران دارای این نقش
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
