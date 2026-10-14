using System.Net;
using FiapCloudGames.Notifications.IntegrationTests.Support;

namespace FiapCloudGames.Notifications.IntegrationTests.Host;

public sealed class HealthEndpointTests(NotificationsApiFactory factory)
    : IClassFixture<NotificationsApiFactory>
{
    [Fact]
    public async Task HealthLive_ReturnsOk()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
