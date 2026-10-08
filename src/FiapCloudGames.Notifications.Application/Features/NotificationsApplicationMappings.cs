using FiapCloudGames.Notifications.Domain.Notifications;

namespace FiapCloudGames.Notifications.Application.Features;

internal static class NotificationsApplicationMappings
{
    public static NotificationResult ToResult(Notification notification) =>
        new(
            notification.Id,
            notification.EventType,
            notification.EventId,
            notification.RecipientUserId,
            notification.Channel,
            notification.Subject,
            notification.Status,
            notification.Attempts,
            notification.LastAttemptAtUtc,
            notification.CreatedAtUtc);
}
