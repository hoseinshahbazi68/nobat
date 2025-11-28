namespace Nobat.Application.Users.Dto;

/// <summary>
/// DTO نقش
/// </summary>
public class RoleDto
{
    /// <summary>
    /// شناسه نقش
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام نقش
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات نقش
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO ایجاد نقش
/// </summary>
public class CreateRoleDto
{
    /// <summary>
    /// نام نقش
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات نقش
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO به‌روزرسانی نقش
/// </summary>
public class UpdateRoleDto
{
    /// <summary>
    /// شناسه نقش
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام نقش
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات نقش
    /// </summary>
    public string? Description { get; set; }
}
