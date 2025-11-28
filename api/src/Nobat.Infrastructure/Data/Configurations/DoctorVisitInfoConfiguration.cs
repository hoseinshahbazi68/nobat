using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Doctors;

namespace Nobat.Infrastructure.Data.Configurations;

public class DoctorVisitInfoConfiguration : IEntityTypeConfiguration<DoctorVisitInfo>
{
    public void Configure(EntityTypeBuilder<DoctorVisitInfo> builder)
    {
        builder.ToTable("DoctorVisitInfos");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DoctorId)
            .IsRequired();

        builder.Property(d => d.About)
            .HasMaxLength(2000);

        builder.Property(d => d.ClinicAddress)
            .HasMaxLength(500);

        builder.Property(d => d.ClinicPhone)
            .HasMaxLength(20);

        builder.Property(d => d.OfficeHours)
            .HasMaxLength(200);

        // رابطه با Doctor (one-to-one)
        builder.HasOne(d => d.Doctor)
            .WithOne(d => d.VisitInfo)
            .HasForeignKey<DoctorVisitInfo>(d => d.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // ایندکس برای DoctorId
        builder.HasIndex(d => d.DoctorId)
            .IsUnique();
    }
}
