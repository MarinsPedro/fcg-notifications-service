namespace FiapCloudGames.Notifications.Worker.Events;

public sealed record OrderCompletedV1
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string GameName { get; init; } = string.Empty;
}
