namespace FiapCloudGames.Notifications.Worker.Events;

public sealed record OrderFailedV1
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
