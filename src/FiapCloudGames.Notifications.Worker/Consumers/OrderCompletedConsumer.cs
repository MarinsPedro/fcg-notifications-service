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
    ILogger<OrderCompletedConsumer> logger) : IConsumer<OrderCompletedEventV1>
{
    public async Task Consume(ConsumeContext<OrderCompletedEventV1> context)
    {
        using var scope = logger.BeginMessageScope(context);

        logger.LogInformation(
            "Processando OrderCompletedEventV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var body = templates.Render("OrderCompleted", new Dictionary<string, string>
        {
            ["OrderId"] = context.Message.OrderId.ToString()
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(OrderCompletedEventV1),
            EventId: context.Message.EventId,
            RecipientUserId: context.Message.UserId,
            Subject: "Pedido concluído",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
