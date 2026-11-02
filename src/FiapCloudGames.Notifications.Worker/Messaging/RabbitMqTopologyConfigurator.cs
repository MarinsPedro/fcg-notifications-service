using FiapCloudGames.Notifications.Worker.Events;
using MassTransit;

namespace FiapCloudGames.Notifications.Worker.Messaging;

internal static class RabbitMqTopologyConfigurator
{
    internal static void Configure(
        IRabbitMqBusFactoryConfigurator rabbitMq,
        RabbitMqExchangeOptions exchanges)
    {
        rabbitMq.Message<UserCreatedEvent>(configuration =>
            configuration.SetEntityName(exchanges.UserCreated));
        rabbitMq.Message<PaymentApprovedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.PaymentApproved));
        rabbitMq.Message<PaymentRejectedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.PaymentRejected));
        rabbitMq.Message<OrderCompletedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.OrderCompleted));
        rabbitMq.Message<OrderFailedEventV1>(configuration =>
            configuration.SetEntityName(exchanges.OrderFailed));
    }
}
