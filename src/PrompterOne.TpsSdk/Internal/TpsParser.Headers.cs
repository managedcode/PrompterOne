using System.Globalization;
using System.Text.RegularExpressions;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal sealed partial class TpsParser
{
    private static ParsedHeader? TryParseHeader(LineRecord line, HeaderLevel level, IReadOnlyList<int> lineStarts, List<TpsDiagnostic> diagnostics)
    {
        var hashPrefix = level == HeaderLevel.Segment ? "##" : "###";
        var trimmedStart = line.Text.TrimStart();
        if (!trimmedStart.StartsWith(hashPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        var afterHashes = trimmedStart[hashPrefix.Length..];
        if (afterHashes.Length > 0 && !afterHashes.StartsWith(' '))
        {
            return null;
        }

        var headerContent = afterHashes.Trim();
        if (headerContent.Length == 0)
        {
            diagnostics.Add(TpsSupport.CreateDiagnostic(
                TpsSpec.DiagnosticCodes.InvalidHeader,
                "Header cannot be empty.",
                line.StartOffset,
                line.StartOffset + line.Text.Length,
                lineStarts));
            return null;
        }

        if (!headerContent.StartsWith("[", StringComparison.Ordinal) || !headerContent.EndsWith("]", StringComparison.Ordinal))
        {
            return new ParsedHeader(headerContent, line.StartOffset, line.StartOffset + line.Text.Length);
        }

        var parsed = ParseBracketHeader(headerContent[1..^1], line.StartOffset + line.Text.IndexOf('[', StringComparison.Ordinal) + 1, lineStarts, diagnostics);
        return parsed is null
            ? null
            : parsed with
            {
                HeaderStart = line.StartOffset,
                HeaderEnd = line.StartOffset + line.Text.Length
            };
    }

    private static ParsedHeader? ParseBracketHeader(string content, int contentOffset, IReadOnlyList<int> lineStarts, List<TpsDiagnostic> diagnostics)
    {
        var parts = TpsEscaping.SplitHeaderPartsDetailed(content);
        if (string.IsNullOrWhiteSpace(parts[0].Value))
        {
            diagnostics.Add(TpsSupport.CreateDiagnostic(
                TpsSpec.DiagnosticCodes.InvalidHeader,
                "Header name is required.",
                contentOffset,
                contentOffset + content.Length,
                lineStarts));
            return null;
        }

        var header = new ParsedHeader(parts[0].Value, contentOffset, contentOffset + content.Length);
        foreach (var part in parts.Skip(1))
        {
            var normalized = TpsSupport.NormalizeValue(part.Value);
            if (normalized is null)
            {
                continue;
            }

            var tokenStart = contentOffset + part.Start;
            var tokenEnd = contentOffset + part.End;
            if (normalized.StartsWith(TpsSpec.SpeakerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                header = header with { Speaker = TpsSupport.NormalizeValue(normalized[TpsSpec.SpeakerPrefix.Length..]) };
                continue;
            }

            if (normalized.StartsWith(TpsSpec.ArchetypePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var archetypeValue = TpsSupport.NormalizeValue(normalized[TpsSpec.ArchetypePrefix.Length..]);
                if (archetypeValue is not null && TpsSpec.Archetypes.Contains(archetypeValue, StringComparer.OrdinalIgnoreCase))
                {
                    header = header with { Archetype = archetypeValue.ToLowerInvariant() };
                }
                else
                {
                    diagnostics.Add(TpsSupport.CreateDiagnostic(
                        TpsSpec.DiagnosticCodes.UnknownArchetype,
                        $"Archetype '{archetypeValue ?? ""}' is not a known vocal archetype.",
                        tokenStart, tokenEnd, lineStarts,
                        "Use one of: Friend, Motivator, Educator, Coach, Storyteller, Entertainer."));
                }

                continue;
            }

            if (TpsSupport.IsTimingToken(normalized))
            {
                header = header with { Timing = normalized };
                continue;
            }

            if (ApplyHeaderWpm(ref header, normalized, tokenStart, tokenEnd, lineStarts, diagnostics))
            {
                continue;
            }

            if (TpsSupport.IsKnownEmotion(normalized.ToLowerInvariant()))
            {
                header = header with { Emotion = normalized.ToLowerInvariant() };
                continue;
            }

            diagnostics.Add(TpsSupport.CreateDiagnostic(
                TpsSpec.DiagnosticCodes.InvalidHeaderParameter,
                $"Header parameter '{normalized}' is not a known TPS header token.",
                tokenStart,
                tokenEnd,
                lineStarts,
                "Use a speaker, emotion, timing, or WPM value."));
        }

        return header;
    }

    private static bool ApplyHeaderWpm(
        ref ParsedHeader header,
        string token,
        int start,
        int end,
        IReadOnlyList<int> lineStarts,
        List<TpsDiagnostic> diagnostics)
    {
        var normalized = string.Concat(token.Where(character => !char.IsWhiteSpace(character)));
        if (!HeaderWpmRegex().IsMatch(normalized))
        {
            return false;
        }

        var candidate = normalized.EndsWith(TpsSpec.WpmSuffix, StringComparison.OrdinalIgnoreCase)
            ? int.Parse(normalized[..^TpsSpec.WpmSuffix.Length], CultureInfo.InvariantCulture)
            : int.Parse(normalized, CultureInfo.InvariantCulture);
        if (candidate < TpsSpec.MinimumWpm || candidate > TpsSpec.MaximumWpm)
        {
            diagnostics.Add(TpsSupport.CreateDiagnostic(
                TpsSpec.DiagnosticCodes.InvalidWpm,
                $"WPM '{token}' must be between {TpsSpec.MinimumWpm} and {TpsSpec.MaximumWpm}.",
                start,
                end,
                lineStarts));
            return true;
        }

        header = header with { TargetWpm = candidate };
        return true;
    }

    private static ParsedSegmentInternal CreateSegment(ParsedHeader header, IReadOnlyDictionary<string, string> metadata, int index)
    {
        var emotion = TpsSupport.ResolveEmotion(header.Emotion);
        var palette = TpsSupport.ResolvePalette(emotion);
        var blocks = new List<TpsBlock>();
        var targetWpm = header.TargetWpm
            ?? (header.Archetype is not null && TpsSpec.ArchetypeRecommendedWpm.TryGetValue(header.Archetype, out var archetypeWpm) ? archetypeWpm : TpsSupport.ResolveBaseWpm(metadata));
        return new ParsedSegmentInternal(
            new TpsSegment
            {
                Id = $"segment-{index}",
                Name = header.Name,
                TargetWpm = targetWpm,
                Emotion = emotion,
                Speaker = header.Speaker,
                Archetype = header.Archetype,
                Timing = header.Timing,
                BackgroundColor = palette.Background,
                TextColor = palette.Text,
                AccentColor = palette.Accent,
                Blocks = blocks
            },
            blocks)
        {
            HeaderStart = header.HeaderStart,
            HeaderEnd = header.HeaderEnd
        };
    }

    private static ParsedSegmentInternal CreateImplicitSegment(IReadOnlyDictionary<string, string> metadata, int index) =>
        CreateSegment(new ParsedHeader(metadata.TryGetValue(TpsSpec.FrontMatterKeys.Title, out var title) ? title : TpsSpec.DefaultImplicitSegmentName, 0, 0)
        {
            TargetWpm = TpsSupport.ResolveBaseWpm(metadata),
            Emotion = TpsSpec.DefaultEmotion
        }, metadata, index);

    private static ParsedBlockInternal CreateBlock(ParsedHeader header, int blockIndex, string segmentId) =>
        new(new TpsBlock
        {
            Id = $"{segmentId}-block-{blockIndex}",
            Name = header.Name,
            TargetWpm = header.TargetWpm,
            Emotion = header.Emotion,
            Speaker = header.Speaker,
            Archetype = header.Archetype
        })
        {
            HeaderStart = header.HeaderStart,
            HeaderEnd = header.HeaderEnd
        };

    [GeneratedRegex(@"^\d+(wpm)?$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex HeaderWpmRegex();
}
