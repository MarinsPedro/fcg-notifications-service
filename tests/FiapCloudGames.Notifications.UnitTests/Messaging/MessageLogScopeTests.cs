using FiapCloudGames.Notifications.Worker.Logging;

namespace FiapCloudGames.Notifications.UnitTests.Messaging;

public sealed class MessageLogScopeTests
{
    [Fact]
    public void Create_WithAllIdentifiers_ExposesThemByName()
    {
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var scope = MessageLogScope.Create(messageId, correlationId, conversationId);

        Assert.Equal(3, scope.Count);
        Assert.Equal(messageId, scope["MessageId"]);
        Assert.Equal(correlationId, scope["CorrelationId"]);
        Assert.Equal(conversationId, scope["ConversationId"]);
    }

    [Fact]
    public void Create_WithOnlyMessageId_OmitsTheOthers()
    {
        var messageId = Guid.NewGuid();

        var scope = MessageLogScope.Create(messageId, null, null);

        Assert.Single(scope);
        Assert.Equal(messageId, scope["MessageId"]);
        Assert.False(scope.ContainsKey("CorrelationId"));
        Assert.False(scope.ContainsKey("ConversationId"));
    }

    [Fact]
    public void Create_WithoutIdentifiers_ReturnsEmptyScope()
    {
        var scope = MessageLogScope.Create(null, null, null);

        Assert.Empty(scope);
    }
}
