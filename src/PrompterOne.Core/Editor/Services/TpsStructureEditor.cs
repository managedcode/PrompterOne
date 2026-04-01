using System.Text.RegularExpressions;
using PrompterOne.Core.Models.Editor;

namespace PrompterOne.Core.Services.Editor;

public sealed class TpsStructureEditor
{
    private const string BlockPrefix = "### ";
    private const string SegmentPrefix = "## ";
    private const string WpmSuffix = "WPM";

    private static readonly Regex BlockHeaderRegex = new(
        @"^###\s*\[(?<name>[^\]|]+?)(?:\|(?<wpm>\d+)WPM)?(?:\|(?<emotion>[^\]|]+))?\]\s*$",
        RegexOptions.Compiled);

    private static readonly Regex SegmentHeaderRegex = new(
        @"^##\s*\[(?<name>[^\]|]+?)(?:\|(?<wpm>\d+)WPM)?(?:\|(?<emotion>[^\]|]+))?(?:\|(?<timing>[^\]]+))?\]\s*$",
        RegexOptions.Compiled);

    public bool TryRead(string? source, int lineStartIndex, out TpsStructureHeaderSnapshot snapshot)
    {
        snapshot = new TpsStructureHeaderSnapshot(TpsStructureHeaderKind.Segment, 0, 0, string.Empty, null, string.Empty, string.Empty);
        var safeSource = source ?? string.Empty;
        if (string.IsNullOrWhiteSpace(safeSource))
        {
            return false;
        }

        var lineRange = ResolveLineRange(safeSource, lineStartIndex);
        var line = safeSource[lineRange.Start..lineRange.End];

        if (TryMatchHeader(line, lineRange.Start, SegmentHeaderRegex, TpsStructureHeaderKind.Segment, out snapshot))
        {
            return true;
        }

        return TryMatchHeader(line, lineRange.Start, BlockHeaderRegex, TpsStructureHeaderKind.Block, out snapshot);
    }

    public EditorTextMutationResult Update(string? source, TpsStructureHeaderSnapshot snapshot)
    {
        var safeSource = source ?? string.Empty;
        var lineRange = ResolveLineRange(safeSource, snapshot.LineStartIndex);
        var normalizedName = string.IsNullOrWhiteSpace(snapshot.Name)
            ? GetFallbackName(snapshot.Kind)
            : snapshot.Name.Trim();
        var replacement = BuildHeaderLine(snapshot with { Name = normalizedName });
        var updated = string.Concat(
            safeSource[..lineRange.Start],
            replacement,
            safeSource[lineRange.End..]);
        var selectionEnd = lineRange.Start + replacement.Length;

        return new EditorTextMutationResult(
            updated,
            new EditorSelectionRange(lineRange.Start, selectionEnd));
    }

    private static string BuildHeaderLine(TpsStructureHeaderSnapshot snapshot)
    {
        var parts = new List<string> { snapshot.Name.Trim() };
        if (snapshot.TargetWpm is int wpm)
        {
            parts.Add($"{wpm}{WpmSuffix}");
        }

        if (!string.IsNullOrWhiteSpace(snapshot.EmotionKey))
        {
            parts.Add(snapshot.EmotionKey.Trim().ToLowerInvariant());
        }

        if (snapshot.SupportsTiming && !string.IsNullOrWhiteSpace(snapshot.Timing))
        {
            parts.Add(snapshot.Timing.Trim());
        }

        var prefix = snapshot.Kind == TpsStructureHeaderKind.Segment ? SegmentPrefix : BlockPrefix;
        return $"{prefix}[{string.Join('|', parts)}]";
    }

    private static string GetFallbackName(TpsStructureHeaderKind kind) =>
        kind == TpsStructureHeaderKind.Segment ? "Segment" : "Block";

    private static (int Start, int End) ResolveLineRange(string source, int lineStartIndex)
    {
        var safeStart = Math.Clamp(lineStartIndex, 0, source.Length);
        var start = safeStart;
        while (start > 0 && source[start - 1] != '\n')
        {
            start--;
        }

        var end = safeStart;
        while (end < source.Length && source[end] != '\n')
        {
            end++;
        }

        return (start, end);
    }

    private static bool TryMatchHeader(
        string line,
        int lineStartIndex,
        Regex pattern,
        TpsStructureHeaderKind kind,
        out TpsStructureHeaderSnapshot snapshot)
    {
        var match = pattern.Match(line);
        if (!match.Success)
        {
            snapshot = new TpsStructureHeaderSnapshot(kind, 0, 0, string.Empty, null, string.Empty, string.Empty);
            return false;
        }

        snapshot = new TpsStructureHeaderSnapshot(
            kind,
            lineStartIndex,
            lineStartIndex + line.Length,
            match.Groups["name"].Value.Trim(),
            TryGetWpm(match.Groups["wpm"].Value),
            match.Groups["emotion"].Value.Trim().ToLowerInvariant(),
            kind == TpsStructureHeaderKind.Segment ? match.Groups["timing"].Value.Trim() : string.Empty);
        return true;
    }

    private static int? TryGetWpm(string value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}
