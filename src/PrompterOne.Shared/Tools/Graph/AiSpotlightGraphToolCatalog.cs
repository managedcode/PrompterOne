using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightGraphToolCatalog
{
    public static void AddTo(List<AiSpotlightTool> tools, ScriptArticleContext context)
    {
        if (context.Editor is null)
        {
            return;
        }

        tools.Add(new AiSpotlightTool(
            AiSpotlightToolNames.GraphInspect,
            AiSpotlightSuggestionKind.Graph,
            UiTextKey.AiSpotlightSuggestionInspectGraph,
            UiTextKey.AiSpotlightSuggestionInspectGraphDetail,
            AiSpotlightToolText.GraphInspect,
            AiSpotlightToolDispatchKinds.Agent,
            AiSpotlightToolScopes.Graph,
            Parameters: []));

        AddReadOnly(tools, AiSpotlightToolNames.GraphOpen, UiTextKey.EditorWorkspaceGraphTab, UiTextKey.AiSpotlightSuggestionInspectGraphDetail, AiSpotlightToolText.GraphOpen);
        AddReadOnly(tools, AiSpotlightToolNames.GraphSummaryRead, UiTextKey.EditorGraphNodesFormat, UiTextKey.EditorGraphLinksFormat, AiSpotlightToolText.GraphSummaryRead);
        AddReadOnly(tools, AiSpotlightToolNames.GraphSearch, UiTextKey.EditorFindPlaceholder, UiTextKey.AiSpotlightSuggestionInspectGraphDetail, AiSpotlightToolText.GraphSearch, AiSpotlightToolParameterSets.GraphQuery);
        AddReadOnly(tools, AiSpotlightToolNames.GraphFilter, UiTextKey.EditorFindPlaceholder, UiTextKey.EditorWorkspaceGraphTab, AiSpotlightToolText.GraphFilter, AiSpotlightToolParameterSets.GraphQuery);
        AddReadOnly(tools, AiSpotlightToolNames.GraphNodeRead, UiTextKey.EditorGraphNodesFormat, UiTextKey.EditorGraphSourceRangesFormat, AiSpotlightToolText.GraphNodeRead, AiSpotlightToolParameterSets.GraphNode);
        AddReadOnly(tools, AiSpotlightToolNames.GraphNeighborsRead, UiTextKey.EditorGraphLinksFormat, UiTextKey.EditorGraphSourceRangesFormat, AiSpotlightToolText.GraphNeighborsRead, AiSpotlightToolParameterSets.GraphNode);
        AddReadOnly(tools, AiSpotlightToolNames.GraphNodeHighlight, UiTextKey.EditorGraphSourceRangesFormat, UiTextKey.EditorStructureSection, AiSpotlightToolText.GraphNodeHighlight, AiSpotlightToolParameterSets.GraphNode);
        AddReadOnly(tools, AiSpotlightToolNames.GraphSectionExplain, UiTextKey.EditorStructureSection, UiTextKey.EditorStructureActiveSegment, AiSpotlightToolText.GraphSectionExplain);
        AddReadOnly(tools, AiSpotlightToolNames.GraphExport, UiTextKey.CommonExport, UiTextKey.EditorWorkspaceGraphTab, AiSpotlightToolText.GraphExport);
        AddReadOnly(tools, AiSpotlightToolNames.GraphDiff, UiTextKey.EditorGraphLinksFormat, UiTextKey.EditorMetadataHistoryHint, AiSpotlightToolText.GraphDiff);
        AddBuildTool(tools, AiSpotlightToolNames.GraphBuild, AiSpotlightToolText.GraphBuild);
        AddBuildTool(tools, AiSpotlightToolNames.GraphRebuild, AiSpotlightToolText.GraphRebuild);
        AddAnnotationTool(tools, AiSpotlightToolNames.GraphAnnotationAdd, AiSpotlightToolText.GraphAnnotationAdd);
        AddAnnotationTool(tools, AiSpotlightToolNames.GraphAnnotationUpdate, AiSpotlightToolText.GraphAnnotationUpdate);
    }

    private static void AddReadOnly(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        UiTextKey detail,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            label,
            detail,
            prompt,
            AiSpotlightToolScopes.Graph,
            parameters));

    private static void AddBuildTool(List<AiSpotlightTool> tools, string name, string prompt) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            UiTextKey.EditorGraphBuilding,
            UiTextKey.EditorWorkspaceGraphTab,
            prompt,
            AiSpotlightToolScopes.Graph,
            readOnly: false,
            idempotent: true));

    private static void AddAnnotationTool(List<AiSpotlightTool> tools, string name, string prompt) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            UiTextKey.EditorWorkspaceGraphTab,
            UiTextKey.EditorGraphSourceRangesFormat,
            prompt,
            AiSpotlightToolScopes.Graph,
            AiSpotlightToolParameterSets.GraphAnnotation,
            readOnly: false,
            idempotent: false,
            requiresApproval: true));
}
