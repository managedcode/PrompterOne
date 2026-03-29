using PrompterLive.Core.Models.Editor;

namespace PrompterLive.Shared.Components.Editor;

public sealed record EditorOutlineSegmentViewModel(
    int Index,
    string Name,
    string EmotionKey,
    string EmotionLabel,
    string AccentColor,
    int TargetWpm,
    string DurationLabel,
    int StartIndex,
    int EndIndex,
    IReadOnlyList<EditorOutlineBlockViewModel> Blocks)
{
    public string TargetWpmLabel => $"{TargetWpm}WPM";
}

public sealed record EditorOutlineBlockViewModel(
    int Index,
    string Name,
    string EmotionLabel,
    int TargetWpm,
    int StartIndex,
    int EndIndex)
{
    public string TargetWpmLabel => $"{TargetWpm}WPM";
}

public sealed record EditorStatusViewModel(
    int Line,
    int Column,
    string Profile,
    int BaseWpm,
    int SegmentCount,
    int BlockCount,
    int WordCount,
    string Duration,
    string Version);

public sealed record EditorNavigationTarget(
    int SegmentIndex,
    int? BlockIndex,
    int StartIndex,
    int EndIndex);

public sealed record EditorSelectionViewModel(
    EditorSelectionRange Range,
    int Line,
    int Column,
    double ToolbarTop,
    double ToolbarLeft)
{
    public static EditorSelectionViewModel Empty { get; } = new(
        EditorSelectionRange.Empty,
        1,
        1,
        0,
        0);

    public bool HasSelection => Range.HasSelection;
}

public enum EditorCommandKind
{
    Wrap,
    Insert
}

public sealed record EditorCommandRequest(
    EditorCommandKind Kind,
    string PrimaryToken,
    string? SecondaryToken = null,
    string PlaceholderText = "",
    int? CaretOffset = null);

public enum EditorHistoryCommand
{
    Undo,
    Redo
}
