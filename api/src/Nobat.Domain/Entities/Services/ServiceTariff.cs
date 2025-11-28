using Nobat.Domain.Common;
using Nobat.Domain.Entities.Clinics;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Entities.Insurances;

namespace Nobat.Domain.Entities.Services;

/// <summary>
/// موجودیت تعرفه خدمت
/// این موجودیت تعرفه خدمات را بر اساس بیمه، پزشک و کلینیک نگهداری می‌کند
/// </summary>
public class ServiceTariff : BaseEntity
{
    /// <summary>
    /// شناسه خدمت
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// خدمت
    /// </summary>
    public virtual Service Service { get; set; } = null!;

    /// <summary>
    /// شناسه بیمه
    /// </summary>
    public int InsuranceId { get; set; }

    /// <summary>
    /// بیمه
    /// </summary>
    public virtual Insurance Insurance { get; set; } = null!;

    /// <summary>
    /// شناسه پزشک (اختیاری)
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// پزشک
    /// </summary>
    public virtual Doctor? Doctor { get; set; }

    /// <summary>
    /// شناسه کلینیک
    /// </summary>
    public int ClinicId { get; set; }

    /// <summary>
    /// کلینیک
    /// </summary>
    public virtual Clinic Clinic { get; set; } = null!;

    /// <summary>
    /// قیمت
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// مدت زمان ویزیت (به دقیقه)
    /// </summary>
    public int? VisitDuration { get; set; }

    /// <summary>
    /// قیمت نهایی (برابر با قیمت پایه)
    /// </summary>
    public decimal FinalPrice => Price;
}
