using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Insurances;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت Insurance
/// این کلاس تنظیمات جدول Insurances در پایگاه داده را تعریف می‌کند
/// شامل محدودیت‌ها، ایندکس‌ها و روابط با موجودیت ServiceTariff
/// </summary>
public class InsuranceConfiguration : IEntityTypeConfiguration<Insurance>
{
    public void Configure(EntityTypeBuilder<Insurance> builder)
    {
        builder.ToTable("Insurances");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.Code)
            .IsUnique();

        builder.HasMany(i => i.ServiceTariffs)
            .WithOne(st => st.Insurance)
            .HasForeignKey(st => st.InsuranceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
