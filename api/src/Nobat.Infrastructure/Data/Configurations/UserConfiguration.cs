using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Users;

namespace Nobat.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.NationalCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.Gender)
            .HasConversion<int>()
            .IsRequired(false);

        builder.Property(u => u.BirthDate)
            .IsRequired(false);

        builder.HasOne(u => u.City)
            .WithMany()
            .HasForeignKey(u => u.CityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(u => u.NationalCode)
            .IsUnique();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
