using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class PaymentApprovedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<PaymentApprovedConsumer> logger) : IConsumer<PaymentApprovedV1>
{
    public async Task Consume(ConsumeContext<PaymentApprovedV1> context)
    {
        logger.LogInformation(
            "Processando PaymentApprovedV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var body = templates.Render("PaymentApproved", new Dictionary<string, string>
        {
            ["OrderId"] = context.Message.OrderId.ToString(),
            ["Amount"] = context.Message.Amount.ToString("N2")
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(PaymentApprovedV1),
            EventId: context.MessageId ?? context.Message.PaymentId,
            RecipientUserId: context.Message.UserId,
            Subject: "Pagamento aprovado",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
