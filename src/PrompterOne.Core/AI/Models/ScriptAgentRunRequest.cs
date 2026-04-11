namespace PrompterOne.Core.AI.Models;

public sealed record ScriptAgentRunRequest(
    string WorkflowId,
    string Input,
    string? ConversationId = null,
    ScriptArticleContext? ArticleContext = null);
