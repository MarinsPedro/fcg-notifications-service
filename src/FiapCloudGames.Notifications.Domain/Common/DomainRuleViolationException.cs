namespace FiapCloudGames.Notifications.Domain.Common;

public sealed class DomainRuleViolationException(string message)
    : Exception(message);
