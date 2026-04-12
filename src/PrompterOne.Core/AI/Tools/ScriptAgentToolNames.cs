namespace PrompterOne.Core.AI.Tools;

public static class ScriptAgentToolNames
{
    public const string GetContext = "get_context";
    public const string ListAppTools = "list_app_tools";
    public const string RequestAppTool = "request_app_tool";
    public const string ReadScriptRange = "read_script_range";
    public const string ReadEditorSelection = "read_editor_selection";
    public const string ProposeScriptReplacement = "propose_script_replacement";
    public const string ProposeScriptInsertion = "propose_script_insertion";
    public const string ProposeScriptDeletion = "propose_script_deletion";
    public const string ApplyApprovedScriptReplacement = "apply_approved_script_replacement";
    public const string ApplyApprovedScriptDeletion = "apply_approved_script_deletion";
    public const string BuildScriptGraphSummary = "build_script_graph_summary";
}

public static class ScriptAgentToolStatuses
{
    public const string Unavailable = "unavailable";
    public const string ApprovalRequired = "approval_required";
    public const string ReadyForNavigationDispatch = "ready_for_navigation_dispatch";
    public const string QueuedForUiDispatch = "queued_for_ui_dispatch";
}

public static class ScriptAgentToolDispatchKinds
{
    public const string Navigation = "navigation";
}
