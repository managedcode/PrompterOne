using System.Globalization;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal static class TpsSupport
{
    private static readonly HashSet<string> Emotions = new(TpsSpec.Emotions, StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> LegacyKeys = new(
    [
        TpsSpec.LegacyKeys.DisplayDuration,
        TpsSpec.LegacyKeys.FastOffset,
        TpsSpec.LegacyKeys.PresetsFast,
        TpsSpec.LegacyKeys.PresetsSlow,
        TpsSpec.LegacyKeys.PresetsXfast,
        TpsSpec.LegacyKeys.PresetsXslow,
        TpsSpec.LegacyKeys.SlowOffset,
        TpsSpec.LegacyKeys.XfastOffset,
        TpsSpec.LegacyKeys.XslowOffset
    ], StringComparer.OrdinalIgnoreCase);

    public static string NormalizeLineEndings(string? value) =>
        value?.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n') ?? string.Empty;

    public static List<int> CreateLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                starts.Add(index + 1);
            }
        }

        return starts;
    }

    public static TpsPosition PositionAt(int offset, IReadOnlyList<int> lineStarts)
    {
        var lineIndex = 0;
        for (var index = 0; index < lineStarts.Count; index++)
        {
            if (lineStarts[index] > offset)
            {
                break;
            }

            lineIndex = index;
        }

        var lineStart = lineStarts[lineIndex];
        return new TpsPosition(lineIndex + 1, offset - lineStart + 1, offset);
    }

    public static TpsDiagnostic CreateDiagnostic(
        string code,
        string message,
        int start,
        int end,
        IReadOnlyList<int> lineStarts,
        string? suggestion = null,
        TpsSeverity? severityOverride = null)
    {
        var severity = severityOverride
            ?? (TpsSpec.WarningDiagnosticCodes.Contains(code) ? TpsSeverity.Warning : TpsSeverity.Error);

        return new TpsDiagnostic(
            code,
            severity,
            message,
            new TpsRange(PositionAt(start, lineStarts), PositionAt(end, lineStarts)),
            suggestion);
    }

    public static TpsDiagnostic CreateWarningDiagnostic(
        string code,
        string message,
        int start,
        int end,
        IReadOnlyList<int> lineStarts,
        string? suggestion = null) =>
        CreateDiagnostic(code, message, start, end, lineStarts, suggestion, TpsSeverity.Warning);

    public static bool HasErrors(IEnumerable<TpsDiagnostic> diagnostics) =>
        diagnostics.Any(diagnostic => diagnostic.Severity == TpsSeverity.Error);

    public static string? NormalizeValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static bool IsLegacyMetadataKey(string key) => LegacyKeys.Contains(key);

    public static bool IsKnownEmotion(string? value) =>
        value is not null && Emotions.Contains(value);

    public static string ResolveEmotion(string? candidate, string? fallback = null)
    {
        var normalized = NormalizeValue(candidate)?.ToLowerInvariant();
        if (normalized is not null && IsKnownEmotion(normalized))
        {
            return normalized;
        }

        return fallback ?? TpsSpec.DefaultEmotion;
    }

    public static EmotionPalette ResolvePalette(string? emotion)
    {
        var key = ResolveEmotion(emotion);
        return TpsSpec.EmotionPalettes[key];
    }

    public static int ResolveBaseWpm(IReadOnlyDictionary<string, string> metadata) =>
        ClampWpm(
            metadata.TryGetValue(TpsSpec.FrontMatterKeys.BaseWpm, out var value) &&
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null,
            TpsSpec.DefaultBaseWpm);

    public static Dictionary<string, int> ResolveSpeedOffsets(IReadOnlyDictionary<string, string> metadata) =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            [TpsSpec.Tags.Xslow] = ResolveSpeedOffset(metadata, TpsSpec.FrontMatterKeys.SpeedOffsetsXslow, TpsSpec.DefaultSpeedOffsets[TpsSpec.Tags.Xslow]),
            [TpsSpec.Tags.Slow] = ResolveSpeedOffset(metadata, TpsSpec.FrontMatterKeys.SpeedOffsetsSlow, TpsSpec.DefaultSpeedOffsets[TpsSpec.Tags.Slow]),
            [TpsSpec.Tags.Fast] = ResolveSpeedOffset(metadata, TpsSpec.FrontMatterKeys.SpeedOffsetsFast, TpsSpec.DefaultSpeedOffsets[TpsSpec.Tags.Fast]),
            [TpsSpec.Tags.Xfast] = ResolveSpeedOffset(metadata, TpsSpec.FrontMatterKeys.SpeedOffsetsXfast, TpsSpec.DefaultSpeedOffsets[TpsSpec.Tags.Xfast])
        };

    public static double? ResolveSpeedMultiplier(string tag, IReadOnlyDictionary<string, int> offsets) =>
        offsets.TryGetValue(tag, out var offset) ? 1d + (offset / 100d) : null;

    public static int? TryParseAbsoluteWpm(string tag)
    {
        if (!tag.EndsWith(TpsSpec.WpmSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return int.TryParse(tag[..^TpsSpec.WpmSuffix.Length], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    public static bool IsTimingToken(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return false;
        }

        return trimmed.Split('-').Length <= 2 && trimmed.Split('-').All(IsTimeToken);
    }

    public static int? TryResolvePauseMilliseconds(string? argument)
    {
        var trimmed = NormalizeValue(argument);
        if (trimmed is null)
        {
            return null;
        }

        if (trimmed.EndsWith("ms", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(trimmed[..^2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var milliseconds)
                ? milliseconds
                : null;
        }

        if (!trimmed.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return double.TryParse(trimmed[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            ? (int)Math.Round(seconds * 1000d, MidpointRounding.AwayFromZero)
            : null;
    }

    public static int CalculateWordDurationMs(string word, int effectiveWpm)
    {
        var baseMilliseconds = 60_000d / Math.Max(1, effectiveWpm);
        return Math.Max(120, (int)Math.Round(baseMilliseconds * (0.8d + (word.Length * 0.04d)), MidpointRounding.AwayFromZero));
    }

    public static int CalculateOrpIndex(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return 0;
        }

        var cleanWord = word.TrimEnd('.', '!', '?', ',', ';', ':', '"', '\'', ')', ']', '}');
        var length = cleanWord.Length;
        if (length <= 1)
        {
            return 0;
        }

        var ratio = length <= 5 ? 0.30d : length <= 9 ? 0.35d : 0.40d;
        return Math.Max(0, Math.Min((int)Math.Floor(length * ratio), length - 1));
    }

    public static int ResolveEffectiveWpm(int inheritedWpm, int? speedOverride, double? speedMultiplier)
    {
        if (speedOverride is int overrideValue)
        {
            return Math.Max(1, overrideValue);
        }

        if (speedMultiplier is double multiplier)
        {
            return Math.Max(1, (int)Math.Round(inheritedWpm * multiplier, MidpointRounding.AwayFromZero));
        }

        return Math.Max(1, inheritedWpm);
    }

    public static bool IsSentenceEndingPunctuation(string value)
    {
        var trimmed = value.TrimEnd();
        return trimmed.EndsWith('.') || trimmed.EndsWith('!') || trimmed.EndsWith('?');
    }

    private static int ResolveSpeedOffset(IReadOnlyDictionary<string, string> metadata, string key, int fallback) =>
        metadata.TryGetValue(key, out var value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;

    private static int ClampWpm(int? candidate, int fallback)
    {
        if (candidate is not int value)
        {
            return fallback;
        }

        return Math.Min(Math.Max(value, TpsSpec.MinimumWpm), TpsSpec.MaximumWpm);
    }

    private static bool IsTimeToken(string value) =>
        TimeSpan.TryParseExact(value.Trim(), ["m\\:ss", "mm\\:ss"], CultureInfo.InvariantCulture, out _);
}
