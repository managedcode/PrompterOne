namespace PrompterOne.Core.AI.Models;

public sealed record ScriptAgentContextSnapshot(
    string? Screen,
    string? Route,
    string? Title,
    string? DocumentId,
    string? DocumentTitle,
    ScriptDocumentRevision Revision,
    ScriptDocumentRange? SelectedRange,
    IReadOnlyList<int> SelectedLineNumbers,
    int ContentLength,
    ScriptKnowledgeGraphContext? Graph,
    IReadOnlyList<ScriptAgentAppToolDescriptor> AvailableTools);

public sealed record ScriptAgentAppToolParameter(
    string Name,
    string Type,
    string Description,
    bool IsRequired = true);

public sealed record ScriptAgentAppToolDescriptor(
    string Name,
    string Title,
    string Description,
    string Scope,
    string DispatchKind,
    string? Route,
    string? HotkeyAction,
    string? Prompt,
    bool ReadOnly,
    bool Idempotent,
    bool Destructive,
    bool OpenWorld,
    bool RequiresApproval,
    IReadOnlyList<ScriptAgentAppToolParameter> Parameters);

public sealed record ScriptAgentRangeReadResult(
    ScriptDocumentRange Range,
    ScriptDocumentPosition Start,
    ScriptDocumentPosition End,
    string Text);

public sealed record ScriptAgentEditPreviewResult(
    string? Reason,
    ScriptDocumentEditPlan Plan);

public sealed record ScriptAgentAppliedEditPreviewResult(
    string? Reason,
    ScriptDocumentEditResult Result);

public sealed record ScriptAgentGraphSummaryResult(
    ScriptDocumentRevision Revision,
    int NodeCount,
    int EdgeCount,
    IReadOnlyList<string> FocusLabels);

public sealed record ScriptAgentRequestedAppToolResult(
    string ToolName,
    string Status,
    string? ArgumentsJson);
