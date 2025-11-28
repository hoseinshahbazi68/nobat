using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Locations;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت City
/// </summary>
public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .HasMaxLength(10);

        builder.HasOne(c => c.Province)
            .WithMany(p => p.Cities)
            .HasForeignKey(c => c.ProvinceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ایندکس ترکیبی برای جلوگیری از تکراری بودن نام شهر در یک استان
        builder.HasIndex(c => new { c.Name, c.ProvinceId })
            .IsUnique();
    }
}
