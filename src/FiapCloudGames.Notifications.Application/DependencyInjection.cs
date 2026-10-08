using FiapCloudGames.Notifications.Application.Features.Get;
using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Application.Features.Retry;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Notifications.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsApplication(this IServiceCollection services)
    {
        services.AddScoped<GetNotificationService>();
        services.AddScoped<RetryNotificationService>();
        services.AddScoped<ProcessNotificationService>();
        return services;
    }
}
