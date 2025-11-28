using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Appointments;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت Appointment
/// </summary>
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);


        builder.Property(a => a.Status)
            .HasConversion<int>();

        // ایندکس برای جستجوی سریع
        builder.HasIndex(a => a.DoctorId);
        builder.HasIndex(a => a.DoctorScheduleId);
        builder.HasIndex(a => a.AppointmentDateTime);
        builder.HasIndex(a => a.Status);

        // رابطه با Doctor
        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // رابطه با DoctorSchedule
        builder.HasOne(a => a.DoctorSchedule)
            .WithMany(ds => ds.Appointments)
            .HasForeignKey(a => a.DoctorScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
