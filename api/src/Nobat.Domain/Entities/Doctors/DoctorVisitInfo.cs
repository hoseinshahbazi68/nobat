using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت اطلاعات ویزیت پزشک
/// این موجودیت اطلاعات مربوط به ویزیت پزشک را نگهداری می‌کند
/// شامل درباره پزشک، آدرس مطب، تلفن مطب و ساعت حضور
/// </summary>
public class DoctorVisitInfo : BaseEntity
{
    /// <summary>
    /// شناسه پزشک
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// پزشک
    /// </summary>
    public virtual Doctor Doctor { get; set; } = null!;

    /// <summary>
    /// درباره پزشک
    /// </summary>
    public string? About { get; set; }

    /// <summary>
    /// آدرس مطب
    /// </summary>
    public string? ClinicAddress { get; set; }

    /// <summary>
    /// تلفن مطب
    /// </summary>
    public string? ClinicPhone { get; set; }

    /// <summary>
    /// ساعت حضور در مطب
    /// </summary>
    public string? OfficeHours { get; set; }
}
