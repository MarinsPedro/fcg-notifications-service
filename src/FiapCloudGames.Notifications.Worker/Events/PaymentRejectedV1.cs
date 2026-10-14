namespace FiapCloudGames.Notifications.Worker.Events;

public sealed record PaymentRejectedV1
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
