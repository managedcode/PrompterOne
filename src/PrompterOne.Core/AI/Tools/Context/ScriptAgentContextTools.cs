using System.ComponentModel;
using ModelContextProtocol.Server;
using PrompterOne.Core.AI.Models;

namespace PrompterOne.Core.AI.Tools;

[McpServerToolType]
internal sealed class ScriptAgentContextTools(ScriptArticleContext context)
{
    [McpServerTool(
        Name = ScriptAgentToolNames.GetContext,
        Title = "Get active PrompterOne context",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Read the active PrompterOne route, editor selection, document revision, and graph summary.")]
    public ScriptAgentContextSnapshot GetActivePrompterContext()
    {
        var content = GetContent();
        var revision = GetRevision(content);

        return new ScriptAgentContextSnapshot(
            context.Screen,
            context.Route,
            context.Title,
            context.Editor?.DocumentId,
            context.Editor?.DocumentTitle,
            revision,
            context.Editor?.SelectedRange,
            context.Editor?.SelectedLineNumbers ?? [],
            content.Length,
            context.Graph,
            context.AvailableTools ?? []);
    }

    [McpServerTool(
        Name = ScriptAgentToolNames.ListAppTools,
        Title = "List available PrompterOne tools",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("List the route-aware PrompterOne app actions available to the assistant.")]
    public IReadOnlyList<ScriptAgentAppToolDescriptor> ListAvailablePrompterOneTools() =>
        context.AvailableTools ?? [];

    [McpServerTool(
        Name = ScriptAgentToolNames.RequestAppTool,
        Title = "Request a PrompterOne UI tool",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Read the dispatch status for one available PrompterOne app tool by name so the browser UI can dispatch or ask for approval.")]
    public ScriptAgentRequestedAppToolResult RequestPrompterOneTool(
        [Description("The stable PrompterOne tool name to request.")]
        string toolName,
        [Description("Optional serialized JSON arguments for the requested tool.")]
        string? argumentsJson = null)
    {
        var tool = (context.AvailableTools ?? [])
            .FirstOrDefault(candidate => string.Equals(candidate.Name, toolName, StringComparison.Ordinal));

        if (tool is null)
        {
            return new ScriptAgentRequestedAppToolResult(
                toolName,
                ScriptAgentToolStatuses.Unavailable,
                argumentsJson);
        }

        return new ScriptAgentRequestedAppToolResult(
            tool.Name,
            ResolveRequestStatus(tool),
            argumentsJson);
    }

    private string GetContent() =>
        context.Editor?.Content ?? context.Content ?? string.Empty;

    private ScriptDocumentRevision GetRevision(string content) =>
        context.Editor?.Revision ?? ScriptDocumentRevision.Create(content);

    private static string ResolveRequestStatus(ScriptAgentAppToolDescriptor tool)
    {
        if (tool.RequiresApproval)
        {
            return ScriptAgentToolStatuses.ApprovalRequired;
        }

        return tool.DispatchKind == ScriptAgentToolDispatchKinds.Navigation
            ? ScriptAgentToolStatuses.ReadyForNavigationDispatch
            : ScriptAgentToolStatuses.QueuedForUiDispatch;
    }
}
