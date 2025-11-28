using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Chat;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت ChatSession
/// </summary>
public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.ToTable("ChatSessions");

        // تنظیم PhoneNumber به عنوان کلید اصلی
        builder.HasKey(c => c.PhoneNumber);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.UserName)
            .HasMaxLength(200);

        // Index برای UserId
        builder.HasIndex(c => c.UserId);

        // رابطه با User (اختیاری)
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // رابطه One-to-Many با ChatMessage
        builder.HasMany(c => c.ChatMessages)
            .WithOne(m => m.ChatSession)
            .HasForeignKey(m => m.PhoneNumber)
            .HasPrincipalKey(c => c.PhoneNumber)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
