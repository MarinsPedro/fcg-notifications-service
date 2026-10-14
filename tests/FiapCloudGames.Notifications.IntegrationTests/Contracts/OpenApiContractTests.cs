using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FiapCloudGames.Notifications.IntegrationTests.Support;

namespace FiapCloudGames.Notifications.IntegrationTests.Contracts;

public sealed class OpenApiContractTests(NotificationsApiFactory factory)
    : IClassFixture<NotificationsApiFactory>
{
    [Fact]
    public async Task Swagger_IsAvailableInDevelopmentAndDocumentsControllers()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Test", "integration");

        using var uiResponse = await client.GetAsync("/swagger/index.html");
        using var documentResponse = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, uiResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, documentResponse.StatusCode);

        await using var stream = await documentResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var paths = document.RootElement.GetProperty("paths");

        Assert.True(paths.TryGetProperty("/internal/v1/notifications/{notificationId}", out _));
        Assert.True(paths.TryGetProperty("/internal/v1/notifications/{notificationId}/retry", out _));

        var securitySchemes = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes");
        Assert.True(securitySchemes.TryGetProperty("bearer", out _));
    }
}
