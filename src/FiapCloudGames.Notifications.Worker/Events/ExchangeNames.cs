namespace FiapCloudGames.Notifications.Worker.Events;

/// <summary>
/// Nomes explícitos dos exchanges do RabbitMQ, no padrão
/// <c>&lt;produtor&gt;.&lt;evento&gt;.v1</c>. Sem isso o MassTransit usa o
/// namespace C# completo do tipo como nome do exchange.
/// </summary>
public static class ExchangeNames
{
    public const string UserCreated = "identity.user-created.v1";
    public const string PaymentApproved = "payments.payment-approved.v1";
    public const string PaymentRejected = "payments.payment-rejected.v1";
    public const string OrderCompleted = "orders.order-completed.v1";
    public const string OrderFailed = "orders.order-failed.v1";
}
