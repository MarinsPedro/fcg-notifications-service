using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FiapCloudGames.Notifications.Worker.Health;

public static class WorkerHealthExtensions
{
    public const string ReadyTag = "ready";

    /// <summary>
    /// Mapeia <c>/health/live</c> (o processo responde, sem rodar checks) e
    /// <c>/health/ready</c> (roda os checks com a tag <c>ready</c>: banco e
    /// RabbitMQ).
    /// </summary>
    public static IEndpointRouteBuilder MapWorkerHealthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions { Predicate = _ => false });
        endpoints.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(ReadyTag)
            });

        return endpoints;
    }
}
