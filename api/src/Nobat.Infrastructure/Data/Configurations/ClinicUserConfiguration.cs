using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Clinics;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت ClinicUser
/// </summary>
public class ClinicUserConfiguration : IEntityTypeConfiguration<ClinicUser>
{
    public void Configure(EntityTypeBuilder<ClinicUser> builder)
    {
        builder.ToTable("ClinicUsers");

        builder.HasKey(cu => cu.Id);

        // رابطه با Clinic
        builder.HasOne(cu => cu.Clinic)
            .WithMany(c => c.ClinicUsers)
            .HasForeignKey(cu => cu.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه با User
        builder.HasOne(cu => cu.User)
            .WithMany(u => u.ClinicUsers)
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ایندکس ترکیبی برای جلوگیری از تکرار
        builder.HasIndex(cu => new { cu.ClinicId, cu.UserId })
            .IsUnique();
    }
}
