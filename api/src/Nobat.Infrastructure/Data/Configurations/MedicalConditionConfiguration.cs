using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Doctors;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت MedicalCondition
/// </summary>
public class MedicalConditionConfiguration : IEntityTypeConfiguration<MedicalCondition>
{
    public void Configure(EntityTypeBuilder<MedicalCondition> builder)
    {
        builder.ToTable("MedicalConditions");

        builder.HasKey(mc => mc.Id);

        builder.Property(mc => mc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(mc => mc.Description)
            .HasMaxLength(1000);

        builder.HasIndex(mc => mc.Name)
            .IsUnique();
    }
}
