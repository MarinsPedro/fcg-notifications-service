using FiapCloudGames.Identity.Contracts.Events.V1;
using FiapCloudGames.Notifications.Application.Abstractions.Delivery;
using FiapCloudGames.Notifications.Application.Abstractions.Persistence;
using FiapCloudGames.Notifications.Application.Features.Process;
using FiapCloudGames.Notifications.Domain.Notifications;
using FiapCloudGames.Notifications.Worker.Consumers;
using FiapCloudGames.Notifications.Worker.Events;
using FiapCloudGames.Notifications.Worker.Templates;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FiapCloudGames.Notifications.UnitTests.Messaging;

public sealed class ConsumersHarnessTests
{
    [Fact]
    public async Task UserCreated_GeneratesWelcomeNotification()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await using var host = await StartAsync();
        await host.Harness.Bus.Publish(new UserCreatedEvent
        {
            EventId = eventId,
            UserId = userId,
            Name = "Alessandra",
            Email = "alessandra@example.com",
            OccurredAtUtc = DateTimeOffset.UtcNow
        });

        Assert.True(await host.Harness.Consumed.Any<UserCreatedEvent>());

        var notification = Assert.Single(host.Repository.Items);
        Assert.Equal(nameof(UserCreatedEvent), notification.EventType);
        Assert.Equal(eventId, notification.EventId);
        Assert.Equal(userId, notification.RecipientUserId);
        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.Contains("Alessandra", notification.Body);
    }

    [Fact]
    public async Task UserCreated_WithSameEventIdTwice_CreatesOnlyOneNotification()
    {
        var eventId = Guid.NewGuid();
        var message = new UserCreatedEvent
        {
            EventId = eventId,
            UserId = Guid.NewGuid(),
            Name = "Alessandra",
            Email = "alessandra@example.com",
            OccurredAtUtc = DateTimeOffset.UtcNow
        };

        await using var host = await StartAsync();
        await host.Harness.Bus.Publish(message);
        await host.Harness.Bus.Publish(message);

        await host.WaitForConsumedAsync<UserCreatedEvent>(2);

        Assert.Single(host.Repository.Items);
    }

    [Fact]
    public async Task PaymentApproved_UsesPaymentIdAsEventIdWhenMessageHasNoId()
    {
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var host = await StartAsync();
        await host.Harness.Bus.Publish(new PaymentApprovedV1
        {
            PaymentId = paymentId,
            OrderId = orderId,
            UserId = userId,
            Amount = 59.9m
        });

        Assert.True(await host.Harness.Consumed.Any<PaymentApprovedV1>());

        var notification = Assert.Single(host.Repository.Items);
        Assert.Equal(nameof(PaymentApprovedV1), notification.EventType);
        Assert.Equal(userId, notification.RecipientUserId);
        Assert.Equal("Pagamento aprovado", notification.Subject);
        Assert.Contains(orderId.ToString(), notification.Body);
    }

    [Fact]
    public async Task PaymentRejected_IncludesFailureReasonInBody()
    {
        var orderId = Guid.NewGuid();

        await using var host = await StartAsync();
        await host.Harness.Bus.Publish(new PaymentRejectedV1
        {
            PaymentId = Guid.NewGuid(),
            OrderId = orderId,
            UserId = Guid.NewGuid(),
            FailureReason = "Saldo insuficiente"
        });

        Assert.True(await host.Harness.Consumed.Any<PaymentRejectedV1>());

        var notification = Assert.Single(host.Repository.Items);
        Assert.Equal("Pagamento não aprovado", notification.Subject);
        Assert.Contains("Saldo insuficiente", notification.Body);
        Assert.Contains(orderId.ToString(), notification.Body);
    }

    [Fact]
    public async Task OrderCompleted_IncludesGameNameInBody()
    {
        await using var host = await StartAsync();
        await host.Harness.Bus.Publish(new OrderCompletedV1
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameName = "Hollow Knight"
        });

        Assert.True(await host.Harness.Consumed.Any<OrderCompletedV1>());

        var notification = Assert.Single(host.Repository.Items);
        Assert.Equal("Pedido concluído", notification.Subject);
        Assert.Contains("Hollow Knight", notification.Body);
    }

    [Fact]
    public async Task OrderFailed_IncludesReasonInBody()
    {
        await using var host = await StartAsync();
        await host.Harness.Bus.Publish(new OrderFailedV1
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Reason = "Jogo indisponível"
        });

        Assert.True(await host.Harness.Consumed.Any<OrderFailedV1>());

        var notification = Assert.Single(host.Repository.Items);
        Assert.Equal("Pedido não concluído", notification.Subject);
        Assert.Contains("Jogo indisponível", notification.Body);
    }

    [Fact]
    public async Task SenderFailure_MarksNotificationAsFailed()
    {
        await using var host = await StartAsync(senderResult: false);
        await host.Harness.Bus.Publish(new OrderCompletedV1
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameName = "Celeste"
        });

        Assert.True(await host.Harness.Consumed.Any<OrderCompletedV1>());

        var notification = Assert.Single(host.Repository.Items);
        Assert.Equal(NotificationStatus.Failed, notification.Status);
    }

    private static async Task<TestHost> StartAsync(bool senderResult = true)
    {
        var repository = new InMemoryNotificationRepository();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(repository);
        services.AddSingleton<INotificationRepository>(repository);
        services.AddSingleton<INotificationsUnitOfWork>(repository);
        services.AddSingleton<INotificationSender>(new FakeSender(senderResult));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<NotificationTemplateRenderer>();
        services.AddScoped<ProcessNotificationService>();
        services.AddMassTransitTestHarness(bus =>
        {
            bus.AddConsumer<UserCreatedConsumer>();
            bus.AddConsumer<PaymentApprovedConsumer>();
            bus.AddConsumer<PaymentRejectedConsumer>();
            bus.AddConsumer<OrderCompletedConsumer>();
            bus.AddConsumer<OrderFailedConsumer>();
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        return new TestHost(provider, harness, repository);
    }

    private sealed class TestHost(
        ServiceProvider provider,
        ITestHarness harness,
        InMemoryNotificationRepository repository) : IAsyncDisposable
    {
        public ITestHarness Harness { get; } = harness;
        public InMemoryNotificationRepository Repository { get; } = repository;

        public async Task WaitForConsumedAsync<T>(int expected)
            where T : class
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < deadline
                && Harness.Consumed.Select<T>().Count() < expected)
            {
                await Task.Delay(50);
            }

            await Harness.InactivityTask;
        }

        public async ValueTask DisposeAsync()
        {
            await Harness.Stop();
            await provider.DisposeAsync();
        }
    }

    private sealed class InMemoryNotificationRepository
        : INotificationRepository, INotificationsUnitOfWork
    {
        private readonly List<Notification> _items = [];

        public IReadOnlyList<Notification> Items => _items.ToArray();

        public Task AddAsync(Notification notification, CancellationToken cancellationToken)
        {
            lock (_items)
            {
                _items.Add(notification);
            }

            return Task.CompletedTask;
        }

        public Task<Notification?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(_items.FirstOrDefault(n => n.Id == id));

        public Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(_items.Any(n => n.EventId == eventId));

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(0);
    }

    private sealed class FakeSender(bool result) : INotificationSender
    {
        public Task<bool> SendAsync(
            NotificationChannel channel,
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken) => Task.FromResult(result);
    }
}
