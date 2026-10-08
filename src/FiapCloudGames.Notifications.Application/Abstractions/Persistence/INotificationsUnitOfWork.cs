namespace FiapCloudGames.Notifications.Application.Abstractions.Persistence;

public interface INotificationsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
