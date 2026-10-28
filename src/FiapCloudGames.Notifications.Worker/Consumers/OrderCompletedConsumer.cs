using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Logging;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class OrderCompletedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<OrderCompletedConsumer> logger) : IConsumer<OrderCompletedV1>
{
    public async Task Consume(ConsumeContext<OrderCompletedV1> context)
    {
        using var scope = logger.BeginMessageScope(context);

        logger.LogInformation(
            "Processando OrderCompletedV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var body = templates.Render("OrderCompleted", new Dictionary<string, string>
        {
            ["OrderId"] = context.Message.OrderId.ToString(),
            ["GameName"] = context.Message.GameName
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(OrderCompletedV1),
            EventId: context.MessageId ?? Guid.NewGuid(),
            RecipientUserId: context.Message.UserId,
            Subject: "Pedido concluído",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
