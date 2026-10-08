using FiapCloudGames.BuildingBlocks.Authentication;
using FiapCloudGames.Notifications.Presentation.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Notifications.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsPresentation(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(DependencyInjection).Assembly);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                NotificationsPolicies.NotificationsRead,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireServiceToken()
                    .RequireClaim(FiapClaimTypes.Scope, "notifications.read"));
            options.AddPolicy(
                NotificationsPolicies.NotificationsRetry,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireServiceToken()
                    .RequireClaim(FiapClaimTypes.Scope, "notifications.retry"));
        });

        return services;
    }
}
