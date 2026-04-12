using System.ComponentModel;
using ModelContextProtocol.Server;
using PrompterOne.Core.AI.Models;
using PrompterOne.Core.AI.Services;

namespace PrompterOne.Core.AI.Tools;

[McpServerToolType]
internal sealed class ScriptAgentDocumentTools(
    ScriptArticleContext context,
    ScriptDocumentEditService documentEditService)
{
    [McpServerTool(
        Name = ScriptAgentToolNames.ReadScriptRange,
        Title = "Read script range",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Read an exact UTF-16 range from the captured script document without changing it.")]
    public ScriptAgentRangeReadResult ReadScriptRange(int start, int end)
    {
        var content = GetContent();
        var range = new ScriptDocumentRange(start, end);
        var text = documentEditService.ReadRange(content, range);

        return new ScriptAgentRangeReadResult(
            range,
            ScriptDocumentPosition.FromOffset(content, range.Start),
            ScriptDocumentPosition.FromOffset(content, range.End),
            text);
    }

    [McpServerTool(
        Name = ScriptAgentToolNames.ReadEditorSelection,
        Title = "Read editor selection",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Read the current editor selection captured in the agent context.")]
    public ScriptAgentRangeReadResult? ReadEditorSelection()
    {
        var range = context.Editor?.SelectedRange;
        if (range is null)
        {
            return null;
        }

        return ReadScriptRange(range.Value.Start, range.Value.End);
    }

    [McpServerTool(
        Name = ScriptAgentToolNames.ProposeScriptReplacement,
        Title = "Propose script replacement",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Create a revision-bound replacement plan for an exact script range.")]
    public ScriptAgentEditPreviewResult ProposeScriptReplacement(
        [Description("The inclusive UTF-16 start offset for the replacement range.")]
        int start,
        [Description("The exclusive UTF-16 end offset for the replacement range.")]
        int end,
        [Description("The proposed replacement text.")]
        string replacementText,
        [Description("The user-facing reason for the replacement.")]
        string? reason)
    {
        var content = GetContent();
        var plan = CreateReplacementPlan(content, start, end, replacementText);
        return new ScriptAgentEditPreviewResult(reason, plan);
    }

    [McpServerTool(
        Name = ScriptAgentToolNames.ProposeScriptInsertion,
        Title = "Propose script insertion",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Create a revision-bound insertion plan for an exact script offset.")]
    public ScriptAgentEditPreviewResult ProposeScriptInsertion(
        [Description("The UTF-16 offset where the text should be inserted.")]
        int offset,
        [Description("The proposed text to insert.")]
        string insertedText,
        [Description("The user-facing reason for the insertion.")]
        string? reason)
    {
        var content = GetContent();
        var plan = CreateEditPlan(content, [ScriptDocumentEditOperation.Insert(offset, insertedText)]);
        return new ScriptAgentEditPreviewResult(reason, plan);
    }

    [McpServerTool(
        Name = ScriptAgentToolNames.ProposeScriptDeletion,
        Title = "Propose script deletion",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Create a revision-bound deletion plan for an exact script range.")]
    public ScriptAgentEditPreviewResult ProposeScriptDeletion(
        [Description("The inclusive UTF-16 start offset for the deletion range.")]
        int start,
        [Description("The exclusive UTF-16 end offset for the deletion range.")]
        int end,
        [Description("The user-facing reason for the deletion.")]
        string? reason)
    {
        var content = GetContent();
        var range = new ScriptDocumentRange(start, end);
        var plan = CreateEditPlan(content, [ScriptDocumentEditOperation.Delete(range)]);
        return new ScriptAgentEditPreviewResult(reason, plan);
    }

    [McpServerTool(
        Name = ScriptAgentToolNames.ApplyApprovedScriptReplacement,
        Title = "Apply script replacement",
        ReadOnly = false,
        Idempotent = false,
        Destructive = false,
        OpenWorld = false)]
    [Description("Apply a revision-bound replacement to the captured script text and return the updated text.")]
    public ScriptAgentAppliedEditPreviewResult ApplyApprovedScriptReplacement(
        [Description("The inclusive UTF-16 start offset for the replacement range.")]
        int start,
        [Description("The exclusive UTF-16 end offset for the replacement range.")]
        int end,
        [Description("The approved replacement text.")]
        string replacementText,
        [Description("The expected document revision to guard against stale edits.")]
        string expectedRevision,
        [Description("The user-facing reason for the replacement.")]
        string? reason)
    {
        var content = GetContent();
        var plan = CreateReplacementPlan(content, start, end, replacementText, expectedRevision);
        var result = documentEditService.Apply(content, plan);
        return new ScriptAgentAppliedEditPreviewResult(reason, result);
    }

    [McpServerTool(
        Name = ScriptAgentToolNames.ApplyApprovedScriptDeletion,
        Title = "Apply approved script deletion",
        ReadOnly = false,
        Idempotent = false,
        Destructive = true,
        OpenWorld = false)]
    [Description("Apply a revision-bound deletion to the captured script text and return the updated text.")]
    public ScriptAgentAppliedEditPreviewResult ApplyApprovedScriptDeletion(
        [Description("The inclusive UTF-16 start offset for the deletion range.")]
        int start,
        [Description("The exclusive UTF-16 end offset for the deletion range.")]
        int end,
        [Description("The expected document revision to guard against stale edits.")]
        string expectedRevision,
        [Description("The user-facing reason for the deletion.")]
        string? reason)
    {
        var content = GetContent();
        var range = new ScriptDocumentRange(start, end);
        var plan = CreateEditPlan(content, [ScriptDocumentEditOperation.Delete(range)], expectedRevision);
        var result = documentEditService.Apply(content, plan);
        return new ScriptAgentAppliedEditPreviewResult(reason, result);
    }

    private ScriptDocumentEditPlan CreateReplacementPlan(string content, int start, int end, string replacementText, string? expectedRevision = null)
    {
        var range = new ScriptDocumentRange(start, end);
        return CreateEditPlan(content, [ScriptDocumentEditOperation.Replace(range, replacementText)], expectedRevision);
    }

    private ScriptDocumentEditPlan CreateEditPlan(string content, IReadOnlyList<ScriptDocumentEditOperation> operations, string? expectedRevision = null)
    {
        foreach (var operation in operations)
        {
            operation.Range.ValidateWithin(content.Length);
        }

        var revision = string.IsNullOrWhiteSpace(expectedRevision)
            ? GetRevision(content)
            : new ScriptDocumentRevision(expectedRevision);

        return new ScriptDocumentEditPlan(revision, operations, context.Editor?.DocumentId);
    }

    private string GetContent() =>
        context.Editor?.Content ?? context.Content ?? string.Empty;

    private ScriptDocumentRevision GetRevision(string content) =>
        context.Editor?.Revision ?? ScriptDocumentRevision.Create(content);
}
