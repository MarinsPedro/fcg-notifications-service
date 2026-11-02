using MassTransit;

namespace FiapCloudGames.Notifications.Worker.Events;

[MessageUrn("OrderFailedEventV1", useDefaultPrefix: true)]
public sealed record OrderFailedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    string Reason,
    DateTimeOffset OccurredAtUtc);
