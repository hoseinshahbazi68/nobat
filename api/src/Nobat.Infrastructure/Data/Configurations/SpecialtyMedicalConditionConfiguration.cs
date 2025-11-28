using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Doctors;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت SpecialtyMedicalCondition
/// </summary>
public class SpecialtyMedicalConditionConfiguration : IEntityTypeConfiguration<SpecialtyMedicalCondition>
{
    public void Configure(EntityTypeBuilder<SpecialtyMedicalCondition> builder)
    {
        builder.ToTable("SpecialtyMedicalConditions");

        builder.HasKey(smc => smc.Id);

        // ایجاد ایندکس یکتا برای جلوگیری از تکرار
        builder.HasIndex(smc => new { smc.SpecialtyId, smc.MedicalConditionId })
            .IsUnique();

        // رابطه با Specialty
        builder.HasOne(smc => smc.Specialty)
            .WithMany(s => s.SpecialtyMedicalConditions)
            .HasForeignKey(smc => smc.SpecialtyId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه با MedicalCondition
        builder.HasOne(smc => smc.MedicalCondition)
            .WithMany(mc => mc.SpecialtyMedicalConditions)
            .HasForeignKey(smc => smc.MedicalConditionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
