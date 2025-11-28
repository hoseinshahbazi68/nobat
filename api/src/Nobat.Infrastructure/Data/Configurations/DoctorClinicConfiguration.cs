using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Doctors;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت DoctorClinic
/// </summary>
public class DoctorClinicConfiguration : IEntityTypeConfiguration<DoctorClinic>
{
    public void Configure(EntityTypeBuilder<DoctorClinic> builder)
    {
        builder.ToTable("DoctorClinics");

        builder.HasKey(dc => dc.Id);

        // رابطه با Doctor
        builder.HasOne(dc => dc.Doctor)
            .WithMany(d => d.DoctorClinics)
            .HasForeignKey(dc => dc.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه با Clinic
        builder.HasOne(dc => dc.Clinic)
            .WithMany(c => c.DoctorClinics)
            .HasForeignKey(dc => dc.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // ایندکس ترکیبی برای جلوگیری از تکرار
        builder.HasIndex(dc => new { dc.DoctorId, dc.ClinicId })
            .IsUnique();
    }
}
