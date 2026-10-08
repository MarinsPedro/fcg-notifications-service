using FiapCloudGames.Notifications.Domain.Notifications;

namespace FiapCloudGames.Notifications.Application.Abstractions.Delivery;

public interface INotificationSender
{
    Task<bool> SendAsync(
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken);
}
