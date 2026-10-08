using FiapCloudGames.BuildingBlocks.Exceptions;
using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Domain.Notifications;

namespace FiapCloudGames.Notifications.Application.Features.Retry;

public sealed class RetryNotificationService(
    INotificationRepository notifications,
    INotificationsUnitOfWork unitOfWork)
{
    public async Task<NotificationResult> ExecuteAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.FindByIdAsync(notificationId, cancellationToken)
            ?? throw AppException.NotFound("Notificação não encontrada.");

        if (!notification.CanRetry)
        {
            throw AppException.Conflict(
                "A notificação não pode ser reprocessada.");
        }

        notification.ResetForRetry();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return NotificationsApplicationMappings.ToResult(notification);
    }
}
