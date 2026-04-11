namespace PrompterOne.Core.AI.Models;

public sealed record ScriptAgentContext(
    string? ConversationId = null,
    ScriptArticleContext? ArticleContext = null);
