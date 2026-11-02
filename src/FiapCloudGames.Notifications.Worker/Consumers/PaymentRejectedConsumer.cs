using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Logging;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class PaymentRejectedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<PaymentRejectedConsumer> logger) : IConsumer<PaymentRejectedEventV1>
{
    public async Task Consume(ConsumeContext<PaymentRejectedEventV1> context)
    {
        using var scope = logger.BeginMessageScope(context);

        logger.LogInformation(
            "Processando PaymentRejectedEventV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var body = templates.Render("PaymentRejected", new Dictionary<string, string>
        {
            ["OrderId"] = context.Message.OrderId.ToString(),
            ["Reason"] = context.Message.RejectionReason
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(PaymentRejectedEventV1),
            EventId: context.Message.EventId,
            RecipientUserId: context.Message.UserId,
            Subject: "Pagamento não aprovado",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
