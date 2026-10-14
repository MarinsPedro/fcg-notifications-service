using System.Net;
using FiapCloudGames.Notifications.IntegrationTests.Support;

namespace FiapCloudGames.Notifications.IntegrationTests.Contracts;

public sealed class NotificationEndpointTests(NotificationsApiFactory factory)
    : IClassFixture<NotificationsApiFactory>
{
    [Fact]
    public async Task Get_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();
        using var response = await client.GetAsync($"/internal/v1/notifications/{id}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Retry_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        var id = Guid.NewGuid();
        using var response = await client.PostAsync(
            $"/internal/v1/notifications/{id}/retry", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
