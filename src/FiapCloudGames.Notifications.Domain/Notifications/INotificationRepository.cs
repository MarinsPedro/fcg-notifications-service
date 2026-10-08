namespace FiapCloudGames.Notifications.Domain.Notifications;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);
    Task<Notification?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken);
}
