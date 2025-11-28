using Nobat.Domain.Common;

namespace Nobat.Domain.Entities.Doctors;

/// <summary>
/// موجودیت علائم و مشکلات پزشکی
/// این موجودیت علائم و بیماری‌هایی که بیماران ممکن است داشته باشند را نگهداری می‌کند
/// </summary>
public class MedicalCondition : BaseEntity
{
    /// <summary>
    /// نام علائم یا مشکل
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات علائم
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// مجموعه تخصص‌های مرتبط با این علائم (many-to-many)
    /// </summary>
    public virtual ICollection<SpecialtyMedicalCondition> SpecialtyMedicalConditions { get; set; } = new List<SpecialtyMedicalCondition>();

    /// <summary>
    /// مجموعه پزشکان مرتبط با این علائم (many-to-many)
    /// </summary>
    public virtual ICollection<DoctorMedicalCondition> DoctorMedicalConditions { get; set; } = new List<DoctorMedicalCondition>();
}
