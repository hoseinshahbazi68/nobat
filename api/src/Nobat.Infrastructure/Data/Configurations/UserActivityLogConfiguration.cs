using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Users;

namespace Nobat.Infrastructure.Data.Configurations;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.ToTable("UserActivityLogs");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Controller)
            .HasMaxLength(100);

        builder.Property(u => u.HttpMethod)
            .HasMaxLength(10);

        builder.Property(u => u.RequestPath)
            .HasMaxLength(500);

        builder.Property(u => u.IpAddress)
            .HasMaxLength(50);

        builder.Property(u => u.UserAgent)
            .HasMaxLength(500);

        builder.Property(u => u.AdditionalData)
            .HasColumnType("nvarchar(max)");

        builder.Property(u => u.ErrorMessage)
            .HasColumnType("nvarchar(max)");

        builder.Property(u => u.Username)
            .HasMaxLength(100);

        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => u.ActivityTime);
        builder.HasIndex(u => u.Action);

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
