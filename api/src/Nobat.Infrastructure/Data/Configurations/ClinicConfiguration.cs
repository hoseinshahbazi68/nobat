using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Clinics;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت Clinic
/// </summary>
public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("Clinics");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.AppointmentGenerationDays)
            .IsRequired(false);

        // ایندکس برای نام کلینیک (برای جستجوی سریع‌تر)
        builder.HasIndex(c => c.Name);

        // رابطه با City
        builder.HasOne(c => c.City)
            .WithMany()
            .HasForeignKey(c => c.CityId)
            .OnDelete(DeleteBehavior.SetNull);

        // رابطه با DoctorClinics
        builder.HasMany(c => c.DoctorClinics)
            .WithOne(dc => dc.Clinic)
            .HasForeignKey(dc => dc.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه با ClinicUsers
        builder.HasMany(c => c.ClinicUsers)
            .WithOne(cu => cu.Clinic)
            .HasForeignKey(cu => cu.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
