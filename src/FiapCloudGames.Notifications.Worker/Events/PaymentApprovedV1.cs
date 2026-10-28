namespace FiapCloudGames.Notifications.Worker.Events;

/// <summary>
/// Pagamento aprovado. Lido do JSON puro de <c>PaymentPaidEventV1</c>, publicado
/// pelo Payments com a routing key <c>payments.paid</c>; os nomes das
/// propriedades acompanham o contrato do Payments.
/// </summary>
public sealed record PaymentApprovedV1
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
}
