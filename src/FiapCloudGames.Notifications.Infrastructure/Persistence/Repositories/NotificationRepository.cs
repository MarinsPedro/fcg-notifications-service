using FiapCloudGames.Notifications.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Notifications.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository(NotificationsDbContext context) : INotificationRepository
{
    public Task AddAsync(Notification notification, CancellationToken cancellationToken) =>
        context.Notifications.AddAsync(notification, cancellationToken).AsTask();

    public Task<Notification?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Notifications.SingleOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<Notification?> FindByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken) =>
        context.Notifications.SingleOrDefaultAsync(
            notification => notification.EventId == eventId,
            cancellationToken);
}
