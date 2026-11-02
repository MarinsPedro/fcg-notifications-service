using MassTransit;

namespace FiapCloudGames.Notifications.Worker.Events;

[MessageUrn("PaymentApprovedEventV1", useDefaultPrefix: true)]
public sealed record PaymentApprovedEventV1(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    DateTimeOffset OccurredAtUtc);
