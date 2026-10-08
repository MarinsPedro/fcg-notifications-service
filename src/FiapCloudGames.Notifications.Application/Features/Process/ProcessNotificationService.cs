using FiapCloudGames.Notifications.Application.Abstractions.Delivery;
using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Application.Features.Process;

public sealed class ProcessNotificationService(
    INotificationRepository notifications,
    INotificationsUnitOfWork unitOfWork,
    INotificationSender sender,
    TimeProvider clock,
    ILogger<ProcessNotificationService> logger)
{
    public async Task ExecuteAsync(
        ProcessNotificationInput input,
        CancellationToken cancellationToken)
    {
        if (await notifications.ExistsByEventIdAsync(input.EventId, cancellationToken))
        {
            logger.LogInformation(
                "Notificação para evento {EventId} já foi processada. Ignorando duplicata.",
                input.EventId);
            return;
        }

        var notification = Notification.Create(
            input.EventType,
            input.EventId,
            input.RecipientUserId,
            NotificationChannel.Email,
            input.Subject,
            input.Body,
            clock.GetUtcNow());

        await notifications.AddAsync(notification, cancellationToken);

        var sent = await sender.SendAsync(
            notification.Channel,
            notification.RecipientUserId.ToString(),
            notification.Subject,
            notification.Body,
            cancellationToken);

        if (sent)
        {
            notification.MarkSent(clock.GetUtcNow());
        }
        else
        {
            notification.MarkFailed(clock.GetUtcNow());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
