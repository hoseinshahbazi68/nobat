using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Services;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت Service
/// این کلاس تنظیمات جدول Services در پایگاه داده را تعریف می‌کند
/// شامل محدودیت‌ها و روابط با موجودیت ServiceTariff
/// </summary>
public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.HasMany(s => s.ServiceTariffs)
            .WithOne(st => st.Service)
            .HasForeignKey(st => st.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
