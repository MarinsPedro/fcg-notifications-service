namespace FiapCloudGames.Notifications.Worker.Events;

/// <summary>
/// Pagamento recusado. Lido do JSON puro de <c>PaymentFailedEventV1</c>,
/// publicado pelo Payments com a routing key <c>payments.failed</c>; os nomes
/// das propriedades acompanham o contrato do Payments.
/// </summary>
public sealed record PaymentRejectedV1
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public string FailureReason { get; init; } = string.Empty;
}
