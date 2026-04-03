namespace PrompterOne.Core.Models.Editor;

public enum TpsStructureHeaderKind
{
    Segment,
    Block
}

public sealed record TpsStructureHeaderSnapshot(
    TpsStructureHeaderKind Kind,
    int LineStartIndex,
    int LineEndIndex,
    string Name,
    int? TargetWpm,
    string EmotionKey,
    string Speaker,
    string Timing)
{
    public bool SupportsTiming => Kind == TpsStructureHeaderKind.Segment;
}
