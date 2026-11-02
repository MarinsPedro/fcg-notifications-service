using FiapCloudGames.BuildingBlocks.Exceptions;
using FiapCloudGames.Notifications.Application.Abstractions.Delivery;
using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Domain.Common;
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
        var notification = await notifications.FindByEventIdAsync(
            input.EventId,
            cancellationToken);

        if (notification is not null && notification.Status != NotificationStatus.Pending)
        {
            logger.LogInformation(
                "Notificação para evento {EventId} já foi processada. Ignorando duplicata.",
                input.EventId);
            return;
        }

        if (notification is null)
        {
            try
            {
                notification = Notification.Create(
                    input.EventType,
                    input.EventId,
                    input.RecipientUserId,
                    NotificationChannel.Email,
                    input.Subject,
                    input.Body,
                    clock.GetUtcNow());
            }
            catch (DomainRuleViolationException exception)
            {
                throw AppException.Validation(exception.Message);
            }

            await notifications.AddAsync(notification, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            logger.LogInformation(
                "Retomando envio pendente da notificação do evento {EventId}.",
                input.EventId);
        }

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
