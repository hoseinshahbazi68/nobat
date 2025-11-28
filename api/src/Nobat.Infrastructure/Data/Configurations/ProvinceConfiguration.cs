using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Locations;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت Province
/// </summary>
public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("Provinces");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Code)
            .HasMaxLength(10);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasMany(p => p.Cities)
            .WithOne(c => c.Province)
            .HasForeignKey(c => c.ProvinceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
