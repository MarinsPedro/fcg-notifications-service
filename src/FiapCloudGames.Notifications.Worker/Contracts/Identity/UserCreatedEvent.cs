namespace FiapCloudGames.Identity.Contracts.Events.V1;

/// <summary>
/// Cópia do contrato publicado pelo Identity na exchange
/// <c>identity.user-created.v1</c>. O MassTransit casa a mensagem pelo namespace
/// e pelo nome do tipo, então os dois precisam permanecer idênticos aos do
/// Identity (<c>FiapCloudGames.Identity.Contracts.Events.V1.UserCreatedEvent</c>).
/// </summary>
public sealed record UserCreatedEvent
{
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; init; }
}
