using MassTransit;

namespace FiapCloudGames.Notifications.Worker.Events;

[MessageUrn("UserCreatedEvent", useDefaultPrefix: true)]
public sealed record UserCreatedEvent(
    Guid EventId,
    Guid UserId,
    string Name,
    string Email,
    DateTimeOffset OccurredAtUtc);
