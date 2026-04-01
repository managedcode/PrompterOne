using System.Text.RegularExpressions;
using PrompterOne.Core.Models.Documents;
using PrompterOne.Shared.Components.Editor;

namespace PrompterOne.Shared.Services.Editor;

public sealed class EditorOutlineBuilder
{
    private static readonly Regex SegmentHeaderRegex = new(
        @"^##\s*\[[^\r\n]+\]",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex BlockHeaderRegex = new(
        @"^###\s*\[[^\r\n]+\]",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public IReadOnlyList<EditorOutlineSegmentViewModel> Build(ScriptData? scriptData, string sourceBody, int sourceOffset)
    {
        if (scriptData?.Segments is not { Length: > 0 } segments)
        {
            return [];
        }

        var segmentRanges = ResolveSegmentRanges(sourceBody, segments.Length);
        return segments
            .Select((segment, segmentIndex) =>
            {
                var emotionKey = ResolveEmotionKey(segment.Emotion);
                var blocks = segment.Blocks is { Length: > 0 }
                    ? segment.Blocks
                    : [new ScriptBlock
                    {
                        Name = string.IsNullOrWhiteSpace(segment.Name) ? $"Block {segmentIndex + 1}" : $"{segment.Name} Block",
                        Emotion = segment.Emotion,
                        WpmOverride = segment.WpmOverride,
                        Content = segment.Content,
                        StartIndex = 0,
                        EndIndex = 0
                    }];
                var segmentRange = GetRange(segmentRanges, segmentIndex, sourceBody.Length);
                var blockRanges = ResolveBlockRanges(sourceBody, segmentRange, blocks.Length);

                return new EditorOutlineSegmentViewModel(
                    segmentIndex,
                    string.IsNullOrWhiteSpace(segment.Name) ? $"Segment {segmentIndex + 1}" : segment.Name,
                    emotionKey,
                    FormatEmotionLabel(emotionKey),
                    ResolveAccentColor(emotionKey),
                    segment.WpmOverride ?? scriptData.TargetWpm,
                    BuildDurationLabel(segment.StartTime, segment.EndTime),
                    segmentRange.Start + sourceOffset,
                    segmentRange.End + sourceOffset,
                    blocks
                        .Select((block, blockIndex) =>
                        {
                            var blockRange = GetRange(blockRanges, blockIndex, segmentRange.End - segmentRange.Start + 1, segmentRange);
                            return new EditorOutlineBlockViewModel(
                                blockIndex,
                                string.IsNullOrWhiteSpace(block.Name) ? $"Block {blockIndex + 1}" : block.Name,
                                block.Emotion is null || string.Equals(block.Emotion, segment.Emotion, StringComparison.OrdinalIgnoreCase)
                                    ? string.Empty
                                    : FormatEmotionLabel(ResolveEmotionKey(block.Emotion)),
                                block.WpmOverride ?? segment.WpmOverride ?? scriptData.TargetWpm,
                                blockRange.Start + sourceOffset,
                                blockRange.End + sourceOffset);
                        })
                        .ToList());
            })
            .ToList();
    }

    private static List<(int Start, int End)> ResolveSegmentRanges(string sourceBody, int expectedCount)
    {
        var matches = SegmentHeaderRegex.Matches(sourceBody);
        if (matches.Count == 0)
        {
            return BuildFallbackRanges(expectedCount, sourceBody.Length);
        }

        return matches
            .Select((match, index) =>
            {
                var start = match.Index;
                var nextStart = index + 1 < matches.Count ? matches[index + 1].Index : sourceBody.Length;
                return (start, Math.Max(start, nextStart - 1));
            })
            .ToList();
    }

    private static List<(int Start, int End)> ResolveBlockRanges(
        string sourceBody,
        (int Start, int End) segmentRange,
        int expectedCount)
    {
        if (segmentRange.Start >= sourceBody.Length || segmentRange.End < segmentRange.Start)
        {
            return BuildFallbackRanges(expectedCount, 0, segmentRange);
        }

        var segmentLength = Math.Min(sourceBody.Length - segmentRange.Start, segmentRange.End - segmentRange.Start + 1);
        var segmentBody = sourceBody.Substring(segmentRange.Start, segmentLength);
        var matches = BlockHeaderRegex.Matches(segmentBody);
        if (matches.Count == 0)
        {
            return BuildFallbackRanges(expectedCount, segmentLength, segmentRange);
        }

        return matches
            .Select((match, index) =>
            {
                var start = segmentRange.Start + match.Index;
                var nextStart = index + 1 < matches.Count
                    ? segmentRange.Start + matches[index + 1].Index
                    : segmentRange.End + 1;
                return (start, Math.Max(start, nextStart - 1));
            })
            .ToList();
    }

    private static List<(int Start, int End)> BuildFallbackRanges(
        int expectedCount,
        int totalLength,
        (int Start, int End)? parentRange = null)
    {
        if (expectedCount <= 0)
        {
            return [];
        }

        var offset = parentRange?.Start ?? 0;
        var safeLength = Math.Max(totalLength, 1);
        var size = Math.Max(1, safeLength / expectedCount);
        var ranges = new List<(int Start, int End)>(expectedCount);

        for (var index = 0; index < expectedCount; index++)
        {
            var start = offset + (index * size);
            var end = index == expectedCount - 1
                ? offset + safeLength - 1
                : Math.Max(start, offset + ((index + 1) * size) - 1);
            ranges.Add((start, end));
        }

        return ranges;
    }

    private static (int Start, int End) GetRange(
        IReadOnlyList<(int Start, int End)> ranges,
        int index,
        int fallbackLength,
        (int Start, int End)? parentRange = null)
    {
        if (index >= 0 && index < ranges.Count)
        {
            return ranges[index];
        }

        var fallbackRanges = BuildFallbackRanges(index + 1, fallbackLength, parentRange);
        return fallbackRanges[index];
    }

    private static string BuildDurationLabel(string? startTime, string? endTime) =>
        !string.IsNullOrWhiteSpace(startTime) && !string.IsNullOrWhiteSpace(endTime)
            ? $"{startTime}-{endTime}"
            : string.Empty;

    private static string ResolveEmotionKey(string? emotion)
    {
        var normalized = string.IsNullOrWhiteSpace(emotion)
            ? "neutral"
            : emotion.Trim().ToLowerInvariant();

        return normalized switch
        {
            "worried" or "serious" or "frustrated" or "empathetic" => "concerned",
            "determined" or "innovative" or "analytical" or "precise" or "stable" or "methodical" => "focused",
            "supportive" or "helpful" or "encouraging" or "inspiring" => "motivational",
            "thoughtful" or "confident" or "satisfied" => "professional",
            "artistic" or "creative" or "amazed" or "impressed" or "passionate" => "excited",
            "grateful" => "warm",
            _ => normalized switch
            {
                "warm" or "concerned" or "focused" or "motivational" or "neutral" or "urgent" or
                "happy" or "excited" or "sad" or "calm" or "energetic" or "professional" => normalized,
                _ => "neutral"
            }
        };
    }

    private static string FormatEmotionLabel(string emotionKey) =>
        emotionKey switch
        {
            "concerned" => "Concerned",
            "motivational" => "Motivational",
            "professional" => "Professional",
            "energetic" => "Energetic",
            _ when string.IsNullOrWhiteSpace(emotionKey) => "Neutral",
            _ => char.ToUpperInvariant(emotionKey[0]) + emotionKey[1..]
        };

    private static string ResolveAccentColor(string emotionKey) =>
        emotionKey switch
        {
            "warm" => "#E97F00",
            "concerned" => "#B91C1C",
            "focused" => "#16A34A",
            "motivational" => "#7C3AED",
            "urgent" => "#DC2626",
            "happy" => "#D97706",
            "excited" => "#DB2777",
            "sad" => "#4F46E5",
            "calm" => "#0D9488",
            "energetic" => "#EA580C",
            "professional" => "#1D4ED8",
            _ => "#2563EB"
        };
}
