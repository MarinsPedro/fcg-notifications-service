using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options), INotificationsUnitOfWork
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
    }
}
