using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Services;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت ServiceTariff
/// این کلاس تنظیمات جدول ServiceTariffs در پایگاه داده را تعریف می‌کند
/// شامل محدودیت‌ها، ایندکس‌های ترکیبی و روابط با Service، Insurance، Doctor و Clinic
/// ایندکس‌های ترکیبی برای جلوگیری از تکراری بودن تعرفه‌ها استفاده می‌شوند
/// </summary>
public class ServiceTariffConfiguration : IEntityTypeConfiguration<ServiceTariff>
{
    public void Configure(EntityTypeBuilder<ServiceTariff> builder)
    {
        builder.ToTable("ServiceTariffs");

        builder.HasKey(st => st.Id);

        builder.Property(st => st.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(st => st.VisitDuration)
            .IsRequired(false);

        builder.HasOne(st => st.Service)
            .WithMany(s => s.ServiceTariffs)
            .HasForeignKey(st => st.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(st => st.Insurance)
            .WithMany(i => i.ServiceTariffs)
            .HasForeignKey(st => st.InsuranceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(st => st.Doctor)
            .WithMany(d => d.ServiceTariffs)
            .HasForeignKey(st => st.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(st => st.Clinic)
            .WithMany()
            .HasForeignKey(st => st.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // ایجاد ایندکس ترکیبی برای جلوگیری از تکراری بودن
        builder.HasIndex(st => new { st.ServiceId, st.InsuranceId, st.DoctorId, st.ClinicId })
            .IsUnique()
            .HasFilter("[DoctorId] IS NOT NULL");

        builder.HasIndex(st => new { st.ServiceId, st.InsuranceId, st.ClinicId })
            .IsUnique()
            .HasFilter("[DoctorId] IS NULL");
    }
}
