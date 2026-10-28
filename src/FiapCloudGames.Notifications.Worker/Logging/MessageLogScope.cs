using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Notifications.Worker.Logging;

/// <summary>
/// Escopo de log com os identificadores de rastreio da mensagem consumida.
/// Permite seguir uma operação entre serviços pelo <c>CorrelationId</c> e achar
/// a mensagem exata pelo <c>MessageId</c>. Só entram os identificadores que o
/// produtor informou.
/// </summary>
public static class MessageLogScope
{
    public static IReadOnlyDictionary<string, object> Create(
        Guid? messageId,
        Guid? correlationId,
        Guid? conversationId)
    {
        var scope = new Dictionary<string, object>();

        if (messageId is { } message)
        {
            scope["MessageId"] = message;
        }

        if (correlationId is { } correlation)
        {
            scope["CorrelationId"] = correlation;
        }

        if (conversationId is { } conversation)
        {
            scope["ConversationId"] = conversation;
        }

        return scope;
    }

    public static IDisposable? BeginMessageScope(this ILogger logger, ConsumeContext context) =>
        logger.BeginScope(Create(context.MessageId, context.CorrelationId, context.ConversationId));
}
