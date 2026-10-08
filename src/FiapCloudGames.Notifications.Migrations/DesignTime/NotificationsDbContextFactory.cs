using FiapCloudGames.Notifications.Infrastructure.Persistence;
using FiapCloudGames.Notifications.Migrations.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FiapCloudGames.Notifications.Migrations.DesignTime;

public sealed class NotificationsDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        var builder = MigrationConnection.CreateBuilder(args);
        var options = new DbContextOptionsBuilder<NotificationsDbContext>();
        MigrationDbContextOptions.Configure(
            options,
            MigrationConnection.GetRequired(builder.Configuration));

        return new NotificationsDbContext(options.Options);
    }
}
