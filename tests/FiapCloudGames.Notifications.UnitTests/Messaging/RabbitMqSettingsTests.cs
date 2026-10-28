using FiapCloudGames.Notifications.Worker.Messaging;
using Microsoft.Extensions.Configuration;

namespace FiapCloudGames.Notifications.UnitTests.Messaging;

public sealed class RabbitMqSettingsTests
{
    private static IConfiguration Build(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Fact]
    public void From_WithoutTheSection_UsesTheDefaults()
    {
        var settings = RabbitMqSettings.From(Build([]));

        Assert.Equal("localhost", settings.Host);
        Assert.Equal((ushort)5672, settings.Port);
        Assert.Equal("/", settings.VirtualHost);
        Assert.Equal("guest", settings.Username);
        Assert.Equal("guest", settings.Password);
    }

    [Fact]
    public void From_WithAllValues_UsesTheConfiguredOnes()
    {
        var settings = RabbitMqSettings.From(Build(new()
        {
            ["RabbitMq:Host"] = "rabbitmq",
            ["RabbitMq:Port"] = "5673",
            ["RabbitMq:VirtualHost"] = "fcg",
            ["RabbitMq:Username"] = "notifications",
            ["RabbitMq:Password"] = "segredo"
        }));

        Assert.Equal("rabbitmq", settings.Host);
        Assert.Equal((ushort)5673, settings.Port);
        Assert.Equal("fcg", settings.VirtualHost);
        Assert.Equal("notifications", settings.Username);
        Assert.Equal("segredo", settings.Password);
    }

    [Fact]
    public void From_WithOnlyTheHost_KeepsTheDefaultsForTheRest()
    {
        var settings = RabbitMqSettings.From(Build(new() { ["RabbitMq:Host"] = "rabbitmq" }));

        Assert.Equal("rabbitmq", settings.Host);
        Assert.Equal((ushort)5672, settings.Port);
        Assert.Equal("/", settings.VirtualHost);
    }
}
