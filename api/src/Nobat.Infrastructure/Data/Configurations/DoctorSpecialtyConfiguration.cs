using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Doctors;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت DoctorSpecialty
/// </summary>
public class DoctorSpecialtyConfiguration : IEntityTypeConfiguration<DoctorSpecialty>
{
    public void Configure(EntityTypeBuilder<DoctorSpecialty> builder)
    {
        builder.ToTable("DoctorSpecialties");

        builder.HasKey(ds => ds.Id);

        // ایجاد ایندکس یکتا برای جلوگیری از تکرار
        builder.HasIndex(ds => new { ds.DoctorId, ds.SpecialtyId })
            .IsUnique();

        // رابطه با Doctor
        builder.HasOne(ds => ds.Doctor)
            .WithMany(d => d.DoctorSpecialties)
            .HasForeignKey(ds => ds.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه با Specialty
        builder.HasOne(ds => ds.Specialty)
            .WithMany()
            .HasForeignKey(ds => ds.SpecialtyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
