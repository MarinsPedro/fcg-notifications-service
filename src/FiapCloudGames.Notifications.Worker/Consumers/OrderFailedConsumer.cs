using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Logging;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class OrderFailedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<OrderFailedConsumer> logger) : IConsumer<OrderFailedEventV1>
{
    public async Task Consume(ConsumeContext<OrderFailedEventV1> context)
    {
        using var scope = logger.BeginMessageScope(context);

        logger.LogInformation(
            "Processando OrderFailedEventV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var body = templates.Render("OrderFailed", new Dictionary<string, string>
        {
            ["OrderId"] = context.Message.OrderId.ToString(),
            ["Reason"] = context.Message.Reason
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(OrderFailedEventV1),
            EventId: context.Message.EventId,
            RecipientUserId: context.Message.UserId,
            Subject: "Pedido não concluído",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
