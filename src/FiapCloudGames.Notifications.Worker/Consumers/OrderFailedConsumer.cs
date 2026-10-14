using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Worker.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Consumers;

public sealed class OrderFailedConsumer(
    ProcessNotificationService service,
    ILogger<OrderFailedConsumer> logger) : IConsumer<OrderFailedV1>
{
    public async Task Consume(ConsumeContext<OrderFailedV1> context)
    {
        logger.LogInformation(
            "Processando OrderFailedV1 para pedido {OrderId}, usuário {UserId}.",
            context.Message.OrderId,
            context.Message.UserId);

        var input = new ProcessNotificationInput(
            EventType: nameof(OrderFailedV1),
            EventId: context.MessageId ?? Guid.NewGuid(),
            RecipientUserId: context.Message.UserId,
            Subject: "Pedido não concluído",
            Body: $"Infelizmente seu pedido {context.Message.OrderId} não pôde ser concluído. Motivo: {context.Message.Reason}. Entre em contato com o suporte se precisar de ajuda.");

        await service.ExecuteAsync(input, context.CancellationToken);
    }
}
