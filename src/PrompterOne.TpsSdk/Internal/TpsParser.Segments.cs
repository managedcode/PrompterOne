using System.Collections.ObjectModel;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal sealed partial class TpsParser
{
    private static List<ParsedSegmentInternal> ParseSegments(
        string body,
        int bodyStartOffset,
        Dictionary<string, string> metadata,
        IReadOnlyList<int> lineStarts,
        List<TpsDiagnostic> diagnostics)
    {
        var segments = new List<ParsedSegmentInternal>();
        var preamble = new List<LineRecord>();
        ParsedSegmentInternal? current = null;
        ParsedBlockInternal? currentBlock = null;
        var segmentLeading = new List<LineRecord>();
        var blockLines = new List<LineRecord>();

        foreach (var line in SplitLines(body, bodyStartOffset))
        {
            var segmentHeader = TryParseHeader(line, HeaderLevel.Segment, lineStarts, diagnostics);
            if (segmentHeader is not null)
            {
                FinalizeBlock(current, currentBlock, blockLines);
                FinalizeSegment(segments, current, segmentLeading);
                current = CreateSegment(segmentHeader, metadata, segments.Count + 1);
                currentBlock = null;
                if (preamble.Count > 0)
                {
                    segmentLeading = [.. preamble];
                    preamble.Clear();
                }

                continue;
            }

            var blockHeader = TryParseHeader(line, HeaderLevel.Block, lineStarts, diagnostics);
            if (blockHeader is not null)
            {
                if (current is null)
                {
                    current = CreateImplicitSegment(metadata, segments.Count + 1);
                    if (preamble.Count > 0)
                    {
                        segmentLeading = [.. preamble];
                        preamble.Clear();
                    }
                }

                FinalizeBlock(current, currentBlock, blockLines);
                currentBlock = CreateBlock(blockHeader, current.ParsedBlocks.Count + 1, current.Segment.Id);
                blockLines = [];
                continue;
            }

            PushContentLine(current, currentBlock, line, preamble, segmentLeading, blockLines);
        }

        if (current is null)
        {
            var implicitSegment = CreateImplicitSegment(metadata, 1);
            implicitSegment.DirectContent = CreateContentSection(preamble);
            implicitSegment.Segment.LeadingContent = implicitSegment.DirectContent?.Text;
            implicitSegment.Segment.Content = implicitSegment.DirectContent?.Text ?? string.Empty;
            return [implicitSegment];
        }

        FinalizeBlock(current, currentBlock, blockLines);
        FinalizeSegment(segments, current, segmentLeading);
        return segments;
    }

    private static void FinalizeBlock(ParsedSegmentInternal? current, ParsedBlockInternal? block, List<LineRecord> lines)
    {
        if (current is null || block is null)
        {
            return;
        }

        block.Content = CreateContentSection(lines);
        block.Block.Content = block.Content?.Text ?? string.Empty;
        current.Blocks.Add(block.Block);
        current.ParsedBlocks.Add(block);
    }

    private static void FinalizeSegment(List<ParsedSegmentInternal> target, ParsedSegmentInternal? segment, List<LineRecord> lines)
    {
        if (segment is null)
        {
            return;
        }

        segment.LeadingContent = CreateContentSection(lines);
        segment.Segment.LeadingContent = segment.LeadingContent?.Text;
        segment.Segment.Content = segment.ParsedBlocks.Count == 0 ? segment.LeadingContent?.Text ?? string.Empty : string.Empty;
        if (segment.ParsedBlocks.Count == 0)
        {
            segment.DirectContent = segment.LeadingContent;
        }

        target.Add(segment);
    }

    private static void PushContentLine(
        ParsedSegmentInternal? current,
        ParsedBlockInternal? currentBlock,
        LineRecord line,
        List<LineRecord> preamble,
        List<LineRecord> segmentLeading,
        List<LineRecord> blockLines)
    {
        if (currentBlock is not null)
        {
            blockLines.Add(line);
        }
        else if (current is not null)
        {
            segmentLeading.Add(line);
        }
        else
        {
            preamble.Add(line);
        }
    }

    private static ContentSection? CreateContentSection(IReadOnlyList<LineRecord> lines)
    {
        if (lines.Count == 0)
        {
            return null;
        }

        return new ContentSection(string.Join('\n', lines.Select(line => line.Text)), lines[0].StartOffset);
    }

    private static IEnumerable<LineRecord> SplitLines(string text, int startOffset)
    {
        if (text.Length == 0)
        {
            yield break;
        }

        var lineStart = startOffset;
        foreach (var line in text.Split('\n'))
        {
            if (line.Length == 0 && lineStart == startOffset + text.Length)
            {
                yield break;
            }

            yield return new LineRecord(line, lineStart);
            lineStart += line.Length + 1;
        }
    }

    private static TpsDocument FreezeDocument(
        IReadOnlyDictionary<string, string> metadata,
        IReadOnlyList<ParsedSegmentInternal> parsedSegments) =>
        new()
        {
            Metadata = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase)),
            Segments = Array.AsReadOnly(parsedSegments.Select(segment => FreezeSegment(segment.Segment)).ToArray())
        };

    private static TpsSegment FreezeSegment(TpsSegment segment) =>
        new()
        {
            Id = segment.Id,
            Name = segment.Name,
            Content = segment.Content,
            TargetWpm = segment.TargetWpm,
            Emotion = segment.Emotion,
            Speaker = segment.Speaker,
            Archetype = segment.Archetype,
            Timing = segment.Timing,
            BackgroundColor = segment.BackgroundColor,
            TextColor = segment.TextColor,
            AccentColor = segment.AccentColor,
            LeadingContent = segment.LeadingContent,
            Blocks = Array.AsReadOnly(segment.Blocks.Select(FreezeBlock).ToArray())
        };

    private static TpsBlock FreezeBlock(TpsBlock block) =>
        new()
        {
            Id = block.Id,
            Name = block.Name,
            Content = block.Content,
            TargetWpm = block.TargetWpm,
            Emotion = block.Emotion,
            Speaker = block.Speaker,
            Archetype = block.Archetype
        };
}
