using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Logging;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class PaymentApprovedConsumer(
    ProcessNotificationService service,
    NotificationTemplateRenderer templates,
    ILogger<PaymentApprovedConsumer> logger) : IConsumer<PaymentApprovedEventV1>
{
    public async Task Consume(ConsumeContext<PaymentApprovedEventV1> context)
    {
        using var scope = logger.BeginMessageScope(context);

        logger.LogInformation(
            "Processando PaymentApprovedEventV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var body = templates.Render("PaymentApproved", new Dictionary<string, string>
        {
            ["OrderId"] = context.Message.OrderId.ToString(),
            ["Amount"] = context.Message.Amount.ToString(
                "N2",
                System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))
        });

        var input = new ProcessNotificationInput(
            EventType: nameof(PaymentApprovedEventV1),
            EventId: context.Message.EventId,
            RecipientUserId: context.Message.UserId,
            Subject: "Pagamento aprovado",
            Body: body);

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
