namespace FiapCloudGames.Notifications.Worker.Events;

public sealed record UserCreatedV1
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
