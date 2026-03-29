using PrompterLive.Core.Models.Documents;
using PrompterLive.Core.Services.Preview;

namespace PrompterLive.Core.Models.Workspace;

public sealed record ScriptWorkspaceState(
    string ScriptId,
    string Title,
    string Text,
    string DocumentName,
    ScriptData? ScriptData,
    PrompterLive.Core.Models.CompiledScript.CompiledScript? CompiledScript,
    IReadOnlyList<SegmentPreviewModel> PreviewSegments,
    int WordCount,
    TimeSpan EstimatedDuration,
    string? ErrorMessage,
    ReaderSettings ReaderSettings,
    LearnSettings LearnSettings)
{
    public bool HasContent => WordCount > 0;

    public static ScriptWorkspaceState Empty { get; } = new(
        ScriptId: string.Empty,
        Title: "Fresh Take",
        Text: string.Empty,
        DocumentName: "fresh-take.tps",
        ScriptData: null,
        CompiledScript: null,
        PreviewSegments: Array.Empty<SegmentPreviewModel>(),
        WordCount: 0,
        EstimatedDuration: TimeSpan.Zero,
        ErrorMessage: null,
        ReaderSettings: new ReaderSettings(),
        LearnSettings: new LearnSettings());
}
