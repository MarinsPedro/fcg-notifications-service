using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class OrderFailedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<OrderFailedConsumer> logger) : IConsumer<OrderFailedV1>
{
    public async Task Consume(ConsumeContext<OrderFailedV1> context)
    {
        logger.LogInformation(
            "Processando OrderFailedV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var body = templates.Render("OrderFailed", new Dictionary<string, string>
        {
            ["OrderId"] = context.Message.OrderId.ToString(),
            ["Reason"] = context.Message.Reason
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(OrderFailedV1),
            EventId: context.MessageId ?? Guid.NewGuid(),
            RecipientUserId: context.Message.UserId,
            Subject: "Pedido não concluído",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
