namespace FiapCloudGames.Notifications.Application.Features.Process;

public sealed record ProcessNotificationInput(
    string EventType,
    Guid EventId,
    Guid RecipientUserId,
    string Subject,
    string Body);
