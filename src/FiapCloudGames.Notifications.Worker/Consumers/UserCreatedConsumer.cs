using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class UserCreatedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<UserCreatedConsumer> logger) : IConsumer<UserCreatedV1>
{
    public async Task Consume(ConsumeContext<UserCreatedV1> context)
    {
        logger.LogInformation(
            "Processando UserCreatedV1 para usuário {UserId} ({Email}).",
            context.Message.UserId,
            context.Message.Email);

        var body = templates.Render("UserCreated", new Dictionary<string, string>
        {
            ["UserName"] = context.Message.UserName,
            ["Email"] = context.Message.Email
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(UserCreatedV1),
            EventId: context.MessageId ?? Guid.NewGuid(),
            RecipientUserId: context.Message.UserId,
            Subject: "Bem-vindo à Fiap Cloud Games!",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
