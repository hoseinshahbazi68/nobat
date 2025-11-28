using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Common;

namespace Nobat.Infrastructure.Data.Configurations;

public class DatabaseChangeLogConfiguration : IEntityTypeConfiguration<DatabaseChangeLog>
{
    public void Configure(EntityTypeBuilder<DatabaseChangeLog> builder)
    {
        builder.ToTable("DatabaseChangeLogs");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.TableName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.RecordId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.ChangeType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.ChangedColumns)
            .HasColumnType("nvarchar(max)");

        builder.Property(d => d.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(d => d.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(d => d.AdditionalData)
            .HasColumnType("nvarchar(max)");

        builder.Property(d => d.IpAddress)
            .HasMaxLength(50);

        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => d.TableName);
        builder.HasIndex(d => d.ChangeTime);
        builder.HasIndex(d => d.ChangeType);

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
