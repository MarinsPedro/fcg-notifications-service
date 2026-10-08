using FiapCloudGames.Notifications.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.Notifications.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(n => n.EventType).HasColumnName("event_type").HasMaxLength(200).IsRequired();
        builder.Property(n => n.EventId).HasColumnName("event_id").IsRequired();
        builder.HasIndex(n => n.EventId).IsUnique()
            .HasDatabaseName("ix_notifications_event_id");
        builder.Property(n => n.RecipientUserId).HasColumnName("recipient_user_id").IsRequired();
        builder.Property(n => n.Channel).HasColumnName("channel")
            .HasConversion<int>()
            .HasDefaultValue(NotificationChannel.Email)
            .HasSentinel(NotificationChannel.Undefined)
            .IsRequired();
        builder.Property(n => n.Subject).HasColumnName("subject").HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).HasColumnName("body").IsRequired();
        builder.Property(n => n.Status).HasColumnName("status")
            .HasConversion<int>()
            .HasDefaultValue(NotificationStatus.Pending)
            .HasSentinel(NotificationStatus.Undefined)
            .IsRequired();
        builder.Property(n => n.Attempts).HasColumnName("attempts").IsRequired();
        builder.Property(n => n.LastAttemptAtUtc).HasColumnName("last_attempt_at_utc");
        builder.Property(n => n.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
    }
}
