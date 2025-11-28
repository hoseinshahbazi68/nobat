using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobat.Domain.Entities.Chat;

namespace Nobat.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی Entity Framework برای موجودیت ChatMessage
/// </summary>
public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.SenderType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("User");

        builder.Property(c => c.SenderName)
            .HasMaxLength(200);

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(c => c.IsRead)
            .HasDefaultValue(false);

        // رابطه Many-to-One با ChatSession
        builder.HasOne(c => c.ChatSession)
            .WithMany(s => s.ChatMessages)
            .HasForeignKey(c => c.PhoneNumber)
            .HasPrincipalKey(s => s.PhoneNumber)
            .OnDelete(DeleteBehavior.SetNull);

        // Index برای جستجوی سریع‌تر
        builder.HasIndex(c => c.PhoneNumber);
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.SupportUserId);
        builder.HasIndex(c => c.CreatedAt);
    }
}
