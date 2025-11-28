using Nobat.Domain.Common;
using Nobat.Domain.Entities.Users;

namespace Nobat.Domain.Entities.Clinics;

/// <summary>
/// موجودیت رابطه کلینیک و کاربر (مدیران کلینیک)
/// این موجودیت رابطه many-to-many بین کلینیک‌ها و کاربران را برقرار می‌کند
/// برای مدیریت کلینیک و افزودن پزشکان استفاده می‌شود
/// </summary>
public class ClinicUser : BaseEntity
{
    /// <summary>
    /// شناسه کلینیک
    /// </summary>
    public int ClinicId { get; set; }

    /// <summary>
    /// کلینیک
    /// </summary>
    public virtual Clinic Clinic { get; set; } = null!;

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// کاربر (مدیر کلینیک)
    /// </summary>
    public virtual User User { get; set; } = null!;
}
