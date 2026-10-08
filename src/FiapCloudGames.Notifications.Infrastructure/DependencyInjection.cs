using FiapCloudGames.BuildingBlocks.Authentication;
using FiapCloudGames.Notifications.Application;
using FiapCloudGames.Notifications.Application.Abstractions.Delivery;
using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Domain.Notifications;
using FiapCloudGames.Notifications.Infrastructure.Delivery;
using FiapCloudGames.Notifications.Infrastructure.Health;
using FiapCloudGames.Notifications.Infrastructure.Persistence;
using FiapCloudGames.Notifications.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "A connection string 'Database' não foi configurada.");

        services.AddNotificationsApplication();
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<INotificationsUnitOfWork>(provider =>
            provider.GetRequiredService<NotificationsDbContext>());
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddSingleton<INotificationSender, LogNotificationSender>();
        services.AddHealthChecks()
            .AddCheck<NotificationsDatabaseHealthCheck>(
                "notifications-database",
                tags: ["ready"]);

        AddAuthentication(services, configuration);
        return services;
    }

    private static void AddAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFiapServiceAuthentication(configuration);
    }
}
