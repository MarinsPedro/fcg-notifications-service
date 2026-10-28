namespace FiapCloudGames.Notifications.Worker.Events;

/// <summary>
/// Nomes das exchanges do RabbitMQ e routing keys dos produtores. Sem nome
/// explícito o MassTransit usa o namespace C# completo do tipo como nome do
/// exchange.
/// </summary>
public static class ExchangeNames
{
    // Identity: uma exchange por evento, nomeada com SetEntityName.
    public const string UserCreated = "identity.user-created.v1";

    // Payments: uma exchange topic para todos os eventos, separados por routing key.
    public const string PaymentsEvents = "fcg.payments.events";
    public const string PaymentPaidRoutingKey = "payments.paid";
    public const string PaymentFailedRoutingKey = "payments.failed";

    // Orders: proposta no padrão <produtor>.<evento>.v1 que o Orders já usa.
    // Ainda não há produtor publicando estes dois eventos.
    public const string OrderCompleted = "orders.order-completed.v1";
    public const string OrderFailed = "orders.order-failed.v1";
}
