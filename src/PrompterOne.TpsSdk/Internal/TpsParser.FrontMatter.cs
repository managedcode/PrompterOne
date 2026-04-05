using System.Globalization;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal sealed partial class TpsParser
{
    private static FrontMatterResult ExtractFrontMatter(string source, IReadOnlyList<int> lineStarts, List<TpsDiagnostic> diagnostics)
    {
        if (!source.StartsWith("---\n", StringComparison.Ordinal))
        {
            return new FrontMatterResult(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), source, 0);
        }

        var closing = FindFrontMatterClosing(source);
        if (closing is null)
        {
            diagnostics.Add(TpsSupport.CreateDiagnostic(
                TpsSpec.DiagnosticCodes.InvalidFrontMatter,
                "Front matter must be closed by a terminating --- line.",
                0,
                Math.Min(source.Length, 3),
                lineStarts));
            return new FrontMatterResult(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), source, 0);
        }

        var metadata = ParseMetadata(source[4..closing.Value.Index], 4, lineStarts, diagnostics);
        var bodyStartOffset = closing.Value.Index + closing.Value.Length;
        return new FrontMatterResult(metadata, source[bodyStartOffset..], bodyStartOffset);
    }

    private static Dictionary<string, string> ParseMetadata(
        string frontMatterText,
        int startOffset,
        IReadOnlyList<int> lineStarts,
        List<TpsDiagnostic> diagnostics)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? currentSection = null;
        var lineOffset = startOffset;

        foreach (var rawLine in frontMatterText.Split('\n'))
        {
            var entryStart = lineOffset;
            var entryEnd = lineOffset + rawLine.Length;
            lineOffset = entryEnd + 1;

            if (string.IsNullOrWhiteSpace(rawLine) || rawLine.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var indentation = rawLine.Length - rawLine.TrimStart().Length;
            var line = rawLine.Trim();
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = NormalizeMetadataValue(line[(separatorIndex + 1)..]);
            if (indentation > 0 && currentSection is not null)
            {
                var compositeKey = $"{currentSection}.{key}";
                if (!TpsSupport.IsLegacyMetadataKey(compositeKey))
                {
                    metadata[compositeKey] = value;
                    ValidateMetadataEntry(compositeKey, value, entryStart, entryEnd, lineStarts, diagnostics);
                }

                continue;
            }

            currentSection = value.Length == 0 ? key : null;
            if (value.Length > 0 && !TpsSupport.IsLegacyMetadataKey(key))
            {
                metadata[key] = value;
                ValidateMetadataEntry(key, value, entryStart, entryEnd, lineStarts, diagnostics);
            }
        }

        return metadata;
    }

    private static TitleBodyResult ExtractTitleHeader(string body, int bodyStartOffset, Dictionary<string, string> metadata)
    {
        foreach (var line in SplitLines(body, bodyStartOffset))
        {
            if (string.IsNullOrWhiteSpace(line.Text))
            {
                continue;
            }

            var trimmed = line.Text.Trim();
            if (!trimmed.StartsWith("# ", StringComparison.Ordinal) || trimmed.StartsWith("##", StringComparison.Ordinal))
            {
                break;
            }

            metadata[TpsSpec.FrontMatterKeys.Title] = trimmed[2..].Trim();
            var consumedLength = line.StartOffset - bodyStartOffset + line.Text.Length;
            var trailingNewlineLength = consumedLength < body.Length && body[consumedLength] == '\n' ? 1 : 0;
            var bodyOffset = consumedLength + trailingNewlineLength;
            return new TitleBodyResult(body[bodyOffset..], bodyStartOffset + bodyOffset);
        }

        return new TitleBodyResult(body, bodyStartOffset);
    }

    private static (int Index, int Length)? FindFrontMatterClosing(string source)
    {
        var blockClosingIndex = source.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (blockClosingIndex >= 0)
        {
            return (blockClosingIndex, 5);
        }

        if (source.EndsWith("\n---", StringComparison.Ordinal))
        {
            return (source.Length - 4, 4);
        }

        return null;
    }

    private static void ValidateMetadataEntry(
        string key,
        string value,
        int start,
        int end,
        IReadOnlyList<int> lineStarts,
        List<TpsDiagnostic> diagnostics)
    {
        if (string.Equals(key, TpsSpec.FrontMatterKeys.BaseWpm, StringComparison.Ordinal))
        {
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                diagnostics.Add(TpsSupport.CreateDiagnostic(
                    TpsSpec.DiagnosticCodes.InvalidFrontMatter,
                    "Front matter field 'base_wpm' must be an integer.",
                    start,
                    end,
                    lineStarts));
                return;
            }

            if (parsed < TpsSpec.MinimumWpm || parsed > TpsSpec.MaximumWpm)
            {
                diagnostics.Add(TpsSupport.CreateDiagnostic(
                    TpsSpec.DiagnosticCodes.InvalidWpm,
                    $"WPM '{value}' must be between {TpsSpec.MinimumWpm} and {TpsSpec.MaximumWpm}.",
                    start,
                    end,
                    lineStarts));
            }

            return;
        }

        if (key.StartsWith("speed_offsets.", StringComparison.Ordinal) &&
            !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            diagnostics.Add(TpsSupport.CreateDiagnostic(
                TpsSpec.DiagnosticCodes.InvalidFrontMatter,
                $"Front matter field '{key}' must be an integer.",
                start,
                end,
                lineStarts));
        }
    }

    private static string NormalizeMetadataValue(string value) =>
        value.Trim().Trim('"');
}
