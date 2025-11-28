using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Doctors;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت DoctorSchedule
/// این کلاس تنظیمات جدول DoctorSchedules در پایگاه داده را تعریف می‌کند
/// شامل محدودیت‌ها، ایندکس ترکیبی و روابط با Doctor و Shift
/// ایندکس ترکیبی برای جلوگیری از تکراری بودن برنامه‌ها استفاده می‌شود
/// </summary>
public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
    {
        builder.ToTable("DoctorSchedules");

        builder.HasKey(ds => ds.Id);
 

        builder.HasOne(ds => ds.Doctor)
            .WithMany(d => d.DoctorSchedules)
            .HasForeignKey(ds => ds.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ds => ds.Shift)
            .WithMany()
            .HasForeignKey(ds => ds.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        // رابطه با Clinic (اختیاری)
        builder.HasOne(ds => ds.Clinic)
            .WithMany()
            .HasForeignKey(ds => ds.ClinicId)
            .OnDelete(DeleteBehavior.SetNull);

        // ایندکس برای ClinicId
        builder.HasIndex(ds => ds.ClinicId);

        builder.HasMany(ds => ds.Appointments)
            .WithOne(a => a.DoctorSchedule)
            .HasForeignKey(a => a.DoctorScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

         
    }
}
