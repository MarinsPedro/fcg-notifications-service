using FiapCloudGames.BuildingBlocks.Exceptions;
using FiapCloudGames.Notifications.Domain.Notifications;

namespace FiapCloudGames.Notifications.Application.Features.Get;

public sealed class GetNotificationService(INotificationRepository notifications)
{
    public async Task<NotificationResult> ExecuteAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var notification = await notifications.FindByIdAsync(notificationId, cancellationToken)
            ?? throw AppException.NotFound("Notificação não encontrada.");

        return NotificationsApplicationMappings.ToResult(notification);
    }
}
