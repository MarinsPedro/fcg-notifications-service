using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class PaymentRejectedConsumer(
    ProcessNotificationService service,
    ILogger<PaymentRejectedConsumer> logger) : IConsumer<PaymentRejectedV1>
{
    public async Task Consume(ConsumeContext<PaymentRejectedV1> context)
    {
        logger.LogInformation(
            "Processando PaymentRejectedV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var input = new ProcessNotificationInput(
            EventType: nameof(PaymentRejectedV1),
            EventId: context.MessageId ?? Guid.NewGuid(),
            RecipientUserId: context.Message.UserId,
            Subject: "Pagamento não aprovado",
            Body: $"O pagamento do pedido {context.Message.OrderId} não foi aprovado. Motivo: {context.Message.Reason}. Tente novamente ou entre em contato com o suporte.");

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
