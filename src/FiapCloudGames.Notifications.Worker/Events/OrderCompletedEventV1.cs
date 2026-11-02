using MassTransit;

namespace FiapCloudGames.Notifications.Worker.Events;

[MessageUrn("OrderCompletedEventV1", useDefaultPrefix: true)]
public sealed record OrderCompletedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    DateTimeOffset CompletedAtUtc);
