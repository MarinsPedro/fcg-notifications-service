using FiapCloudGames.Notifications.Domain.Common;
using FiapCloudGames.Notifications.Domain.Notifications;

namespace FiapCloudGames.Notifications.UnitTests.Domain;

public sealed class NotificationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var notification = Notification.Create(
            "PaymentApprovedV1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationChannel.Email,
            "Pagamento aprovado",
            "Seu pagamento foi aprovado.",
            Now);

        Assert.Equal("PaymentApprovedV1", notification.EventType);
        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Equal(0, notification.Attempts);
        Assert.Null(notification.LastAttemptAtUtc);
    }

    [Fact]
    public void MarkSent_SetsStatusAndIncreasesAttempts()
    {
        var notification = CreateValidNotification();

        notification.MarkSent(Now.AddMinutes(1));

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.Equal(1, notification.Attempts);
        Assert.NotNull(notification.LastAttemptAtUtc);
    }

    [Fact]
    public void MarkFailed_SetsStatusAndIncreasesAttempts()
    {
        var notification = CreateValidNotification();

        notification.MarkFailed(Now.AddMinutes(1));

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal(1, notification.Attempts);
    }

    [Fact]
    public void CanRetry_WhenFailedAndBelowMaxAttempts_ReturnsTrue()
    {
        var notification = CreateValidNotification();
        notification.MarkFailed(Now);

        Assert.True(notification.CanRetry);
    }

    [Fact]
    public void CanRetry_WhenSent_ReturnsFalse()
    {
        var notification = CreateValidNotification();
        notification.MarkSent(Now);

        Assert.False(notification.CanRetry);
    }

    [Fact]
    public void PrepareRetry_WhenCanRetry_SetsStatusToPending()
    {
        var notification = CreateValidNotification();
        notification.MarkFailed(Now);

        notification.PrepareRetry();

        Assert.Equal(NotificationStatus.Pending, notification.Status);
    }

    [Fact]
    public void PrepareRetry_WhenAtMaxAttempts_Throws()
    {
        var notification = CreateValidNotification();
        for (var i = 0; i < Notification.MaxAttempts; i++)
        {
            notification.MarkFailed(Now.AddMinutes(i));
            if (i < Notification.MaxAttempts - 1)
            {
                notification.PrepareRetry();
            }
        }

        Assert.False(notification.CanRetry);
        Assert.Throws<DomainRuleViolationException>(() => notification.PrepareRetry());
    }

    [Fact]
    public void Create_WithEmptyEventType_Throws()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            Notification.Create(
                "",
                Guid.NewGuid(),
                Guid.NewGuid(),
                NotificationChannel.Email,
                "Subject",
                "Body",
                Now));
    }

    private static Notification CreateValidNotification() =>
        Notification.Create(
            "PaymentApprovedV1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationChannel.Email,
            "Pagamento aprovado",
            "Seu pagamento foi aprovado.",
            Now);
}
