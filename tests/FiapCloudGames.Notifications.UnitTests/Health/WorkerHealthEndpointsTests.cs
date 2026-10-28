using System.Net;
using FiapCloudGames.Notifications.Worker.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.UnitTests.Health;

public sealed class WorkerHealthEndpointsTests
{
    [Fact]
    public async Task Live_ReturnsOk_EvenWhenReadyCheckFails()
    {
        await using var host = await StartAsync(HealthStatus.Unhealthy);

        using var response = await host.Client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_ReturnsOk_WhenReadyChecksAreHealthy()
    {
        await using var host = await StartAsync(HealthStatus.Healthy);

        using var response = await host.Client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_ReturnsServiceUnavailable_WhenAReadyCheckFails()
    {
        await using var host = await StartAsync(HealthStatus.Unhealthy);

        using var response = await host.Client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private static async Task<TestApp> StartAsync(HealthStatus readyStatus)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddHealthChecks()
            .AddCheck(
                "fake-ready",
                () => new HealthCheckResult(readyStatus),
                tags: [WorkerHealthExtensions.ReadyTag]);

        var app = builder.Build();
        app.MapWorkerHealthEndpoints();
        await app.StartAsync();

        var address = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.First();

        return new TestApp(app, new HttpClient { BaseAddress = new Uri(address) });
    }

    private sealed class TestApp(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }
}
