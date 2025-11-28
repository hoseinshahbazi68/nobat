using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Schedules;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت Holiday
/// این کلاس تنظیمات جدول Holidays در پایگاه داده را تعریف می‌کند
/// شامل محدودیت‌ها و ایندکس‌ها
/// </summary>
public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Date)
            .IsRequired();

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .HasMaxLength(500);

        builder.HasIndex(h => h.Date)
            .IsUnique();
    }
}
