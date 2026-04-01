using PrompterOne.Core.Models.Documents;
using PrompterOne.Core.Services.Preview;

namespace PrompterOne.Core.Models.Workspace;

public sealed record ScriptWorkspaceState(
    string ScriptId,
    string Title,
    string Text,
    string DocumentName,
    ScriptData? ScriptData,
    PrompterOne.Core.Models.CompiledScript.CompiledScript? CompiledScript,
    IReadOnlyList<SegmentPreviewModel> PreviewSegments,
    int WordCount,
    TimeSpan EstimatedDuration,
    string? ErrorMessage,
    ReaderSettings ReaderSettings,
    LearnSettings LearnSettings)
{
    public const string UntitledScriptDocumentName = "untitled-script.tps";
    public const string UntitledScriptTitle = "Untitled Script";

    public bool HasContent => WordCount > 0;

    public static ScriptWorkspaceState Empty { get; } = new(
        ScriptId: string.Empty,
        Title: UntitledScriptTitle,
        Text: string.Empty,
        DocumentName: UntitledScriptDocumentName,
        ScriptData: null,
        CompiledScript: null,
        PreviewSegments: Array.Empty<SegmentPreviewModel>(),
        WordCount: 0,
        EstimatedDuration: TimeSpan.Zero,
        ErrorMessage: null,
        ReaderSettings: new ReaderSettings(),
        LearnSettings: new LearnSettings());
}
