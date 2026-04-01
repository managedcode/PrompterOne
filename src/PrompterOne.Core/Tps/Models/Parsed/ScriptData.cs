namespace PrompterOne.Core.Models.Documents;

public record ScriptData
{
    public string? ScriptId { get; init; }

    public string? Title { get; init; }

    public string? Content { get; init; }

    public bool IsPreview { get; init; }

    public int TargetWpm { get; init; } = 450;

    public ScriptSegment[]? Segments { get; init; }
}

public record ScriptSegment
{
    public required string Name { get; init; }

    public string Emotion { get; init; } = "neutral";

    public string? BackgroundColor { get; init; }

    public string? TextColor { get; init; }

    public string? AccentColor { get; init; }

    public int? WpmOverride { get; init; }

    public int? WpmMax { get; init; }

    public string? StartTime { get; init; }

    public string? EndTime { get; init; }

    public int StartIndex { get; init; }

    public int EndIndex { get; init; }

    public required string Content { get; init; }

    public ScriptBlock[]? Blocks { get; init; }
}

public record ScriptBlock
{
    public required string Name { get; init; }

    public string? Emotion { get; init; }

    public int? WpmOverride { get; init; }

    public int StartIndex { get; init; }

    public int EndIndex { get; init; }

    public required string Content { get; init; }

    public ScriptPhrase[]? Phrases { get; init; }
}

public record ScriptPhrase
{
    public required string Text { get; init; }

    public int StartIndex { get; init; }

    public int EndIndex { get; init; }

    public int? PauseDuration { get; init; }

    public ScriptWord[]? Words { get; init; }
}

public record ScriptWord
{
    public required string Text { get; init; }

    public int OrpIndex { get; init; }

    public int? WpmOverride { get; init; }

    public int EmphasisLevel { get; init; }

    public string? Color { get; init; }

    public int? PauseAfter { get; init; }

    public string? Pronunciation { get; init; }

    public bool IsEditPoint { get; init; }

    public string? EditPointPriority { get; init; }
}
