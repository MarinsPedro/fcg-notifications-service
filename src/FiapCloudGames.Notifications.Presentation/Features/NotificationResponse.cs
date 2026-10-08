using FiapCloudGames.Notifications.Application.Features;

namespace FiapCloudGames.Notifications.Presentation.Features;

public sealed record NotificationResponse(
    Guid Id,
    string EventType,
    Guid EventId,
    Guid RecipientUserId,
    string Channel,
    string Subject,
    string Status,
    int Attempts,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset CreatedAtUtc)
{
    public static NotificationResponse FromResult(NotificationResult result) =>
        new(
            result.Id,
            result.EventType,
            result.EventId,
            result.RecipientUserId,
            result.Channel.ToString(),
            result.Subject,
            result.Status.ToString(),
            result.Attempts,
            result.LastAttemptAtUtc,
            result.CreatedAtUtc);
}
