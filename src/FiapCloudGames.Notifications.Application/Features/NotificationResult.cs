using FiapCloudGames.Notifications.Domain.Notifications;

namespace FiapCloudGames.Notifications.Application.Features;

public sealed record NotificationResult(
    Guid Id,
    string EventType,
    Guid EventId,
    Guid RecipientUserId,
    NotificationChannel Channel,
    string Subject,
    NotificationStatus Status,
    int Attempts,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset CreatedAtUtc);
