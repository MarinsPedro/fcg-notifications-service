using Microsoft.Extensions.Configuration;

namespace FiapCloudGames.Notifications.Worker.Messaging;

/// <summary>
/// Conexão com o RabbitMQ, lida da seção <c>RabbitMq</c> (variáveis
/// <c>RabbitMq__Host</c>, <c>RabbitMq__Port</c>, <c>RabbitMq__VirtualHost</c>,
/// <c>RabbitMq__Username</c> e <c>RabbitMq__Password</c>). As chaves seguem as
/// do Identity e do Orders; todas são opcionais e têm valor padrão.
/// </summary>
public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";

    public ushort Port { get; set; } = 5672;

    public string VirtualHost { get; set; } = "/";

    public string Username { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public static RabbitMqSettings From(IConfiguration configuration) =>
        configuration.GetSection(SectionName).Get<RabbitMqSettings>() ?? new RabbitMqSettings();
}
