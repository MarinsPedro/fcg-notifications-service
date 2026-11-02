using FiapCloudGames.BuildingBlocks.Exceptions;
using FiapCloudGames.Notifications.Application.Abstractions.Delivery;
using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Domain.Common;
using FiapCloudGames.Notifications.Domain.Notifications;

namespace FiapCloudGames.Notifications.Application.Features.Retry;

public sealed class RetryNotificationService(
    INotificationRepository notifications,
    INotificationsUnitOfWork unitOfWork,
    INotificationSender sender,
    TimeProvider clock)
{
    public async Task<NotificationResult> ExecuteAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.FindByIdAsync(notificationId, cancellationToken)
            ?? throw AppException.NotFound("Notificação não encontrada.");

        try
        {
            notification.PrepareRetry();
        }
        catch (DomainRuleViolationException exception)
        {
            throw AppException.Conflict(exception.Message);
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
        return NotificationsApplicationMappings.ToResult(notification);
    }
}
