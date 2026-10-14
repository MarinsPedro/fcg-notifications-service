namespace FiapCloudGames.Notifications.Worker.Events;

public sealed record PaymentApprovedV1
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
}
