using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Doctors;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت DoctorMedicalCondition
/// </summary>
public class DoctorMedicalConditionConfiguration : IEntityTypeConfiguration<DoctorMedicalCondition>
{
    public void Configure(EntityTypeBuilder<DoctorMedicalCondition> builder)
    {
        builder.ToTable("DoctorMedicalConditions");

        builder.HasKey(dmc => dmc.Id);

        // ایجاد ایندکس یکتا برای جلوگیری از تکرار
        builder.HasIndex(dmc => new { dmc.DoctorId, dmc.MedicalConditionId })
            .IsUnique();

        // رابطه با Doctor
        builder.HasOne(dmc => dmc.Doctor)
            .WithMany(d => d.DoctorMedicalConditions)
            .HasForeignKey(dmc => dmc.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه با MedicalCondition
        builder.HasOne(dmc => dmc.MedicalCondition)
            .WithMany(mc => mc.DoctorMedicalConditions)
            .HasForeignKey(dmc => dmc.MedicalConditionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
