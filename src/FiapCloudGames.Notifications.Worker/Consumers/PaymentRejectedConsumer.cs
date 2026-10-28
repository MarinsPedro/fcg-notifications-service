using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class PaymentRejectedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<PaymentRejectedConsumer> logger) : IConsumer<PaymentRejectedV1>
{
    public async Task Consume(ConsumeContext<PaymentRejectedV1> context)
    {
        logger.LogInformation(
            "Processando PaymentRejectedV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var body = templates.Render("PaymentRejected", new Dictionary<string, string>
        {
            ["OrderId"] = context.Message.OrderId.ToString(),
            ["Reason"] = context.Message.FailureReason
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(PaymentRejectedV1),
            EventId: context.MessageId ?? context.Message.PaymentId,
            RecipientUserId: context.Message.UserId,
            Subject: "Pagamento não aprovado",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
