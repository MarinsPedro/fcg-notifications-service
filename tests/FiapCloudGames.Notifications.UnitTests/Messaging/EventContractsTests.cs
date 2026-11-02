using System.Text.Json;
using FiapCloudGames.Notifications.Worker.Events;
using MassTransit;

namespace FiapCloudGames.Notifications.UnitTests.Messaging;

public sealed class EventContractsTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public void PaymentApprovedJson_MapsProducerContract()
    {
        var eventId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "orderId": "{{orderId}}",
              "userId": "{{userId}}",
              "amount": 84.90,
              "occurredAtUtc": "2026-10-27T12:00:00+00:00"
            }
            """;

        var message = JsonSerializer.Deserialize<PaymentApprovedEventV1>(json, Json);

        Assert.NotNull(message);
        Assert.Equal(eventId, message.EventId);
        Assert.Equal(orderId, message.OrderId);
        Assert.Equal(userId, message.UserId);
        Assert.Equal(84.90m, message.Amount);
    }

    [Fact]
    public void PaymentRejectedJson_MapsRejectionReason()
    {
        var eventId = Guid.NewGuid();
        var json = $$"""
            {
              "eventId": "{{eventId}}",
              "orderId": "{{Guid.NewGuid()}}",
              "userId": "{{Guid.NewGuid()}}",
              "amount": 10.00,
              "rejectionReason": "Cartão recusado",
              "occurredAtUtc": "2026-10-27T12:00:00+00:00"
            }
            """;

        var message = JsonSerializer.Deserialize<PaymentRejectedEventV1>(json, Json);

        Assert.NotNull(message);
        Assert.Equal(eventId, message.EventId);
        Assert.Equal("Cartão recusado", message.RejectionReason);
    }

    [Fact]
    public void Contracts_UseNamespaceIndependentMessageUrns()
    {
        Assert.Equal("urn:message:UserCreatedEvent", MessageUrn.ForType<UserCreatedEvent>().ToString());
        Assert.Equal(
            "urn:message:PaymentApprovedEventV1",
            MessageUrn.ForType<PaymentApprovedEventV1>().ToString());
        Assert.Equal(
            "urn:message:PaymentRejectedEventV1",
            MessageUrn.ForType<PaymentRejectedEventV1>().ToString());
        Assert.Equal(
            "urn:message:OrderCompletedEventV1",
            MessageUrn.ForType<OrderCompletedEventV1>().ToString());
    }
}
