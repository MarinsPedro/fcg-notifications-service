using FiapCloudGames.Notifications.Application.Abstractions.Delivery;
using FiapCloudGames.Notifications.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Infrastructure.Delivery;

internal sealed class LogNotificationSender(
    ILogger<LogNotificationSender> logger) : INotificationSender
{
    public Task<bool> SendAsync(
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[FAKE] Enviando {Channel} para {Recipient}: {Subject}",
            channel,
            recipient,
            subject);

        return Task.FromResult(true);
    }
}
