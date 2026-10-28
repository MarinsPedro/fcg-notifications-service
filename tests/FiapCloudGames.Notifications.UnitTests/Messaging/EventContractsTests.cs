using System.Text.Json;
using FiapCloudGames.Notifications.Worker.Events;

namespace FiapCloudGames.Notifications.UnitTests.Messaging;

/// <summary>
/// Garante que os eventos consumidos acompanham o que os produtores publicam.
/// Os exemplos de JSON reproduzem os contratos do Payments
/// (<c>PaymentPaidEventV1</c> e <c>PaymentFailedEventV1</c>) e do Identity
/// (<c>UserCreatedEvent</c>).
/// </summary>
public sealed class EventContractsTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public void PaymentPaidJson_MapsToPaymentApprovedV1()
    {
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var json = $$"""
            {
              "paymentId": "{{paymentId}}",
              "orderId": "{{orderId}}",
              "userId": "{{userId}}",
              "amount": 84.90,
              "providerTransactionId": "tx-123",
              "occurredAtUtc": "2026-10-27T12:00:00+00:00"
            }
            """;

        var message = JsonSerializer.Deserialize<PaymentApprovedV1>(json, Json);

        Assert.NotNull(message);
        Assert.Equal(paymentId, message.PaymentId);
        Assert.Equal(orderId, message.OrderId);
        Assert.Equal(userId, message.UserId);
        Assert.Equal(84.90m, message.Amount);
    }

    [Fact]
    public void PaymentFailedJson_MapsFailureReasonToPaymentRejectedV1()
    {
        var paymentId = Guid.NewGuid();
        var json = $$"""
            {
              "paymentId": "{{paymentId}}",
              "orderId": "{{Guid.NewGuid()}}",
              "userId": "{{Guid.NewGuid()}}",
              "amount": 10.00,
              "failureReason": "Cartão recusado",
              "occurredAtUtc": "2026-10-27T12:00:00+00:00"
            }
            """;

        var message = JsonSerializer.Deserialize<PaymentRejectedV1>(json, Json);

        Assert.NotNull(message);
        Assert.Equal(paymentId, message.PaymentId);
        Assert.Equal("Cartão recusado", message.FailureReason);
    }

    [Fact]
    public void ExchangeNames_MatchWhatTheProducersPublish()
    {
        Assert.Equal("identity.user-created.v1", ExchangeNames.UserCreated);
        Assert.Equal("fcg.payments.events", ExchangeNames.PaymentsEvents);
        Assert.Equal("payments.paid", ExchangeNames.PaymentPaidRoutingKey);
        Assert.Equal("payments.failed", ExchangeNames.PaymentFailedRoutingKey);
    }
}
