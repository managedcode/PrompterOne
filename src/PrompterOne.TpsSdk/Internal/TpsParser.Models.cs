using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal enum HeaderLevel
{
    Segment,
    Block
}

internal sealed record ParsedHeader(
    string Name,
    int HeaderStart,
    int HeaderEnd,
    int? TargetWpm = null,
    string? Emotion = null,
    string? Timing = null,
    string? Speaker = null,
    string? Archetype = null);

internal sealed record LineRecord(string Text, int StartOffset);

internal sealed record ContentSection(string Text, int StartOffset);

internal sealed class ParsedBlockInternal(TpsBlock block)
{
    public TpsBlock Block { get; } = block;

    public int HeaderStart { get; init; }

    public int HeaderEnd { get; init; }

    public ContentSection? Content { get; set; }
}

internal sealed class ParsedSegmentInternal(TpsSegment segment, List<TpsBlock> blocks)
{
    public TpsSegment Segment { get; } = segment;

    public int HeaderStart { get; init; }

    public int HeaderEnd { get; init; }

    public List<TpsBlock> Blocks { get; } = blocks;

    public ContentSection? LeadingContent { get; set; }

    public ContentSection? DirectContent { get; set; }

    public List<ParsedBlockInternal> ParsedBlocks { get; } = [];
}

internal sealed record DocumentAnalysis(
    string Source,
    IReadOnlyList<int> LineStarts,
    List<TpsDiagnostic> Diagnostics,
    TpsDocument Document,
    List<ParsedSegmentInternal> ParsedSegments);

internal sealed record FrontMatterResult(Dictionary<string, string> Metadata, string Body, int BodyStartOffset);

internal sealed record TitleBodyResult(string Body, int BodyStartOffset);
