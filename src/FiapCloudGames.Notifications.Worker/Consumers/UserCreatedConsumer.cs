using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Logging;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class UserCreatedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<UserCreatedConsumer> logger) : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        using var scope = logger.BeginMessageScope(context);

        logger.LogInformation(
            "Processando UserCreatedEvent para usuário {UserId} ({Email}).",
            context.Message.UserId,
            context.Message.Email);

        var body = templates.Render("UserCreated", new Dictionary<string, string>
        {
            ["UserName"] = context.Message.Name,
            ["Email"] = context.Message.Email
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(UserCreatedEvent),
            EventId: context.Message.EventId,
            RecipientUserId: context.Message.UserId,
            Subject: "Bem-vindo à Fiap Cloud Games!",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
