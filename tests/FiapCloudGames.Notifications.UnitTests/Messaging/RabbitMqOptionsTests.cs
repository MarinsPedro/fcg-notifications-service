using FiapCloudGames.Notifications.Worker.Messaging;

namespace FiapCloudGames.Notifications.UnitTests.Messaging;

public sealed class RabbitMqOptionsTests
{
    [Fact]
    public void Defaults_KeepOnlyProtocolDefaults()
    {
        var options = new RabbitMqOptions();

        Assert.Equal((ushort)5672, options.Port);
        Assert.Equal("/", options.VirtualHost);
        Assert.Equal(string.Empty, options.Host);
        Assert.Equal(string.Empty, options.Username);
        Assert.Equal(string.Empty, options.Password);
    }
}
