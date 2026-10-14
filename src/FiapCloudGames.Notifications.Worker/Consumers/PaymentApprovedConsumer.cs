using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class PaymentApprovedConsumer(
    ProcessNotificationService service,
    ILogger<PaymentApprovedConsumer> logger) : IConsumer<PaymentApprovedV1>
{
    public async Task Consume(ConsumeContext<PaymentApprovedV1> context)
    {
        logger.LogInformation(
            "Processando PaymentApprovedV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var input = new ProcessNotificationInput(
            EventType: nameof(PaymentApprovedV1),
            EventId: context.MessageId ?? Guid.NewGuid(),
            RecipientUserId: context.Message.UserId,
            Subject: "Pagamento aprovado",
            Body: $"Seu pagamento do pedido {context.Message.OrderId} no valor de {context.Message.Amount:C} foi aprovado com sucesso.");

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
