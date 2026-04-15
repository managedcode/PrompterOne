using System.ComponentModel;

namespace PrompterOne.Core.AI.Models;

public sealed class ScriptAgentStructuredOutput
{
    [Description("Short text shown to the user in the AI Assistant chat surface.")]
    public string ChatMessage { get; set; } = string.Empty;

    [Description("Exact source document edits to apply after the chat response. Leave empty when no document change is needed.")]
    public List<ScriptAgentStructuredEdit> DocumentEdits { get; set; } = [];
}

public sealed class ScriptAgentStructuredEdit
{
    [Description("Exclusive UTF-16 source offset where the edit ends.")]
    public int End { get; set; }

    [Description("The exact current source text in the range. Required for replace and delete edits.")]
    public string ExpectedText { get; set; } = string.Empty;

    [Description("Edit kind: insert, replace, or delete.")]
    public string Kind { get; set; } = "replace";

    [Description("Inclusive UTF-16 source offset where the edit starts.")]
    public int Start { get; set; }

    [Description("Inserted or replacement text. Leave empty for delete edits.")]
    public string Text { get; set; } = string.Empty;

    public ScriptDocumentEditOperation? ToDocumentEditOperation()
    {
        var normalizedKind = Kind.Trim().ToLowerInvariant();
        return normalizedKind switch
        {
            "insert" => ScriptDocumentEditOperation.Insert(Start, Text),
            "replace" => ScriptDocumentEditOperation.Replace(new ScriptDocumentRange(Start, End), Text),
            "delete" => ScriptDocumentEditOperation.Delete(new ScriptDocumentRange(Start, End)),
            _ => null
        };
    }
}
