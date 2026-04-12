using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightAgentToolCatalog
{
    public static void AddTo(List<AiSpotlightTool> tools, ScriptArticleContext context)
    {
        tools.Add(AiSpotlightToolFactory.AgentTool(
            AiSpotlightToolNames.AskContext,
            UiTextKey.AiSpotlightSuggestionAskContext,
            UiTextKey.AiSpotlightSuggestionAskContextDetail,
            AiSpotlightToolText.AskContext,
            AiSpotlightToolScopes.Global,
            AiSpotlightToolParameterSets.Instruction));

        AddAgentRuntimeTools(tools, context);
    }

    private static void AddAgentRuntimeTools(List<AiSpotlightTool> tools, ScriptArticleContext context)
    {
        AddReadOnly(tools, AiSpotlightToolNames.ContextGet, AiSpotlightToolText.ContextGet);
        AddReadOnly(tools, AiSpotlightToolNames.ToolsList, AiSpotlightToolText.ToolsList);
        AddReadOnly(tools, AiSpotlightToolNames.AgentsList, AiSpotlightToolText.AgentsList);
        AddReadOnly(tools, AiSpotlightToolNames.AgentWorkflowsList, AiSpotlightToolText.AgentWorkflowsList);
        AddReadOnly(tools, AiSpotlightToolNames.AgentContextInspect, AiSpotlightToolText.AgentContextInspect);
        AddWorkflow(tools, AiSpotlightToolNames.WorkflowRunWriter, AiSpotlightToolText.WorkflowRunWriter, requiresApproval: true);
        AddWorkflow(tools, AiSpotlightToolNames.WorkflowRunReviewer, AiSpotlightToolText.WorkflowRunReviewer);
        AddWorkflow(tools, AiSpotlightToolNames.WorkflowRunGroupReview, AiSpotlightToolText.WorkflowRunGroupReview, requiresApproval: true);
        AddReadOnly(tools, AiSpotlightToolNames.AgentOutputsCompare, AiSpotlightToolText.AgentOutputsCompare);
        AddReadOnly(tools, AiSpotlightToolNames.AgentWorkflowsRunningList, AiSpotlightToolText.AgentWorkflowsRunningList);
        AddReadOnly(tools, AiSpotlightToolNames.AgentWorkflowHistoryRead, AiSpotlightToolText.AgentWorkflowHistoryRead);
        AddWorkflow(tools, AiSpotlightToolNames.AgentWorkflowCancel, AiSpotlightToolText.AgentWorkflowCancel);

        AddWorkflow(tools, AiSpotlightToolNames.AgentSpawnScript, AiSpotlightToolText.AgentSpawnScript);
    }

    private static void AddReadOnly(List<AiSpotlightTool> tools, string name, string prompt) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            UiTextKey.HeaderAiSpotlight,
            UiTextKey.AiSpotlightSuggestionAskContextDetail,
            prompt,
            AiSpotlightToolScopes.Agent));

    private static void AddWorkflow(
        List<AiSpotlightTool> tools,
        string name,
        string prompt,
        bool requiresApproval = false) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            UiTextKey.HeaderAiSpotlight,
            UiTextKey.AiSpotlightIdleHint,
            prompt,
            AiSpotlightToolScopes.Agent,
            AiSpotlightToolParameterSets.Workflow,
            readOnly: false,
            idempotent: false,
            openWorld: true,
            requiresApproval: requiresApproval));
}
