using FiapCloudGames.Notifications.Domain.Common;

namespace FiapCloudGames.Notifications.Domain.Notifications;

public sealed class Notification
{
    public const int MaxAttempts = 5;

    private Notification() { }

    private Notification(
        Guid id,
        string eventType,
        Guid eventId,
        Guid recipientUserId,
        NotificationChannel channel,
        string subject,
        string body,
        DateTimeOffset createdAtUtc)
    {
        Id = id != Guid.Empty
            ? id
            : throw new DomainRuleViolationException("O identificador da notificação é obrigatório.");
        EventType = eventType?.Trim() is { Length: > 0 } normalizedEvent
            ? normalizedEvent
            : throw new DomainRuleViolationException("O tipo de evento é obrigatório.");
        EventId = eventId != Guid.Empty
            ? eventId
            : throw new DomainRuleViolationException("O identificador do evento é obrigatório.");
        RecipientUserId = recipientUserId != Guid.Empty
            ? recipientUserId
            : throw new DomainRuleViolationException("O identificador do destinatário é obrigatório.");
        Channel = channel is not NotificationChannel.Undefined && Enum.IsDefined(channel)
            ? channel
            : throw new DomainRuleViolationException("O canal de notificação é inválido.");
        Subject = subject?.Trim() is { Length: > 0 and <= 500 } normalizedSubject
            ? normalizedSubject
            : throw new DomainRuleViolationException("O assunto deve ter entre 1 e 500 caracteres.");
        Body = body?.Trim() is { Length: > 0 } normalizedBody
            ? normalizedBody
            : throw new DomainRuleViolationException("O corpo da notificação é obrigatório.");
        Status = NotificationStatus.Pending;
        Attempts = 0;
        CreatedAtUtc = createdAtUtc != default && createdAtUtc.Offset == TimeSpan.Zero
            ? createdAtUtc
            : throw new DomainRuleViolationException("A data de criação deve estar em UTC.");
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public Guid EventId { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public DateTimeOffset? LastAttemptAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Notification Create(
        string eventType,
        Guid eventId,
        Guid recipientUserId,
        NotificationChannel channel,
        string subject,
        string body,
        DateTimeOffset createdAtUtc) =>
        new(Guid.NewGuid(), eventType, eventId, recipientUserId, channel, subject, body, createdAtUtc);

    public bool CanRetry => Status == NotificationStatus.Failed && Attempts < MaxAttempts;

    public void MarkSent(DateTimeOffset nowUtc)
    {
        CompleteAttempt(NotificationStatus.Sent, nowUtc);
    }

    public void MarkFailed(DateTimeOffset nowUtc)
    {
        CompleteAttempt(NotificationStatus.Failed, nowUtc);
    }

    public void PrepareRetry()
    {
        if (!CanRetry)
        {
            throw new DomainRuleViolationException(
                "A notificação não pode ser reprocessada. Limite de tentativas atingido ou status inválido.");
        }

        Status = NotificationStatus.Pending;
    }

    private void CompleteAttempt(NotificationStatus result, DateTimeOffset nowUtc)
    {
        if (Status != NotificationStatus.Pending)
        {
            throw new DomainRuleViolationException(
                "Somente uma notificação pendente pode concluir uma tentativa de envio.");
        }

        if (nowUtc == default || nowUtc.Offset != TimeSpan.Zero || nowUtc < CreatedAtUtc)
        {
            throw new DomainRuleViolationException(
                "A data da tentativa deve estar em UTC e não pode ser anterior à criação.");
        }

        Attempts++;
        LastAttemptAtUtc = nowUtc;
        Status = result;
    }
}
