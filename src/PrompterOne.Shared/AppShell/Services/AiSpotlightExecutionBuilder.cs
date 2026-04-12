using PrompterOne.Core.AI.Models;

namespace PrompterOne.Shared.Services;

internal static class AiSpotlightExecutionBuilder
{
    public static IReadOnlyList<AiSpotlightLogEntry> BuildRunningLog(ScriptArticleContext context) =>
    [
        new("Context loaded", context.Screen ?? "Route", true),
        new("Graph checked", FormatGraphDetail(context.Graph), true),
        new(
            "Waiting point",
            context.Editor?.SelectedRange is null
                ? "Ready for the next instruction"
                : "Approval required before changing selected text")
    ];

    public static IReadOnlyList<AiSpotlightLogEntry> BuildApprovalLog(AiSpotlightApprovalRequest request) =>
    [
        new("Context loaded", $"Selected range {request.Range.Start}-{request.Range.End}", true),
        new("Prepared range edit", request.Reason, true),
        new("Waiting point", "Review the current and proposed text before applying this edit")
    ];

    public static IReadOnlyList<AiSpotlightLogEntry> AddLog(
        IReadOnlyList<AiSpotlightLogEntry> existing,
        AiSpotlightLogEntry entry) =>
        existing.Concat([entry]).ToArray();

    private static string FormatGraphDetail(ScriptKnowledgeGraphContext? graph) =>
        graph is null || graph.IsEmpty
            ? "No script graph has been built yet"
            : $"{graph.NodeCount} nodes, {graph.EdgeCount} links";
}
