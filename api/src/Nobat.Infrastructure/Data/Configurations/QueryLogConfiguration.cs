using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Common;

namespace Nobat.Infrastructure.Data.Configurations;

public class QueryLogConfiguration : IEntityTypeConfiguration<QueryLog>
{
    public void Configure(EntityTypeBuilder<QueryLog> builder)
    {
        builder.ToTable("QueryLogs");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.QueryText)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(q => q.Parameters)
            .HasColumnType("nvarchar(max)");

        builder.Property(q => q.ExecutionTimeMs)
            .IsRequired();

        builder.Property(q => q.CommandType)
            .HasMaxLength(50);

        builder.Property(q => q.TablesUsed)
            .HasMaxLength(500);

        builder.Property(q => q.IpAddress)
            .HasMaxLength(50);

        builder.Property(q => q.ControllerAction)
            .HasMaxLength(200);

        builder.Property(q => q.ErrorMessage)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(q => q.UserId);
        builder.HasIndex(q => q.ExecutionTime);
        builder.HasIndex(q => q.ExecutionTimeMs);
        builder.HasIndex(q => q.IsHeavy);
        builder.HasIndex(q => q.CommandType);

        // ایجاد ایندکس ترکیبی برای جستجوی سریع‌تر کوئری‌های سنگین
        builder.HasIndex(q => new { q.IsHeavy, q.ExecutionTime });

        builder.HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
