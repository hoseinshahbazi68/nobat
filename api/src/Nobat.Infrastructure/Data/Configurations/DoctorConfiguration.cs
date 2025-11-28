using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Doctors;
using Nobat.Domain.Enums;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت Doctor
/// این کلاس تنظیمات جدول Doctors در پایگاه داده را تعریف می‌کند
/// شامل محدودیت‌ها، ایندکس‌ها و روابط با موجودیت‌های دیگر
/// </summary>
public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.MedicalCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Prefix)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(d => d.ScientificDegree)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(d => d.MedicalCode)
            .IsUnique();

        // ایندکس برای UserId
        builder.HasIndex(d => d.UserId);

        // رابطه با User (اختیاری)
        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.DoctorSchedules)
            .WithOne(ds => ds.Doctor)
            .HasForeignKey(ds => ds.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.ServiceTariffs)
            .WithOne(st => st.Doctor)
            .HasForeignKey(st => st.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.DoctorSpecialties)
            .WithOne(ds => ds.Doctor)
            .HasForeignKey(ds => ds.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // رابطه با DoctorVisitInfo (one-to-one)
        builder.HasOne(d => d.VisitInfo)
            .WithOne(v => v.Doctor)
            .HasForeignKey<DoctorVisitInfo>(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
