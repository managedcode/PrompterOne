using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal static partial class TpsArchetypeAnalyzer
{
    private const string DefaultVolumeLabel = "default";
    private const string UnknownArticulationLabel = "unknown";

    private static bool IsArticulationMismatch(string? value, string expectation)
    {
        if (value is null || string.Equals(expectation, TpsSpec.ArchetypeArticulationExpectations.Flexible, StringComparison.Ordinal))
        {
            return false;
        }

        if (string.Equals(expectation, TpsSpec.ArchetypeArticulationExpectations.Neutral, StringComparison.Ordinal))
        {
            return true;
        }

        return !string.Equals(value, expectation, StringComparison.Ordinal);
    }

    private static bool IsOutOfRange(int? value, NumericRange range) =>
        value is int numeric && (numeric < range.Min || numeric > range.Max);

    private static bool IsWithinRange(double value, NumericRange range) =>
        value >= range.Min && value <= range.Max;

    private static bool IsVolumeMismatch(string? value, string expectation)
    {
        if (string.Equals(expectation, TpsSpec.ArchetypeVolumeExpectations.Flexible, StringComparison.Ordinal) || value is null)
        {
            return false;
        }

        if (string.Equals(expectation, TpsSpec.ArchetypeVolumeExpectations.LoudOnly, StringComparison.Ordinal))
        {
            return !string.Equals(value, TpsSpec.Tags.Loud, StringComparison.Ordinal);
        }

        if (string.Equals(expectation, TpsSpec.ArchetypeVolumeExpectations.DefaultOnly, StringComparison.Ordinal))
        {
            return true;
        }

        return !string.Equals(value, TpsSpec.Tags.Soft, StringComparison.Ordinal);
    }

    private static bool HasInlineSpeedVariation(CompiledWord word, int inheritedWpm) =>
        ResolveEffectiveWordWpm(word, inheritedWpm) != inheritedWpm;

    private static bool IsSpeedMismatch(CompiledWord word, int inheritedWpm, NumericRange range)
    {
        if (word.Metadata.SpeedOverride is null && word.Metadata.SpeedMultiplier is null)
        {
            return false;
        }

        var effectiveWpm = ResolveEffectiveWordWpm(word, inheritedWpm);
        return effectiveWpm < range.Min || effectiveWpm > range.Max;
    }

    private static int ResolveEffectiveWordWpm(CompiledWord word, int inheritedWpm)
    {
        if (word.Metadata.SpeedOverride is int speedOverride)
        {
            return speedOverride;
        }

        if (word.Metadata.SpeedMultiplier is double speedMultiplier)
        {
            return Math.Max(1, (int)Math.Round(inheritedWpm * speedMultiplier, MidpointRounding.AwayFromZero));
        }

        return inheritedWpm;
    }

    private static bool IsSpokenWord(CompiledWord word) =>
        word.Kind == TpsSpec.WordKinds.Word && !string.IsNullOrWhiteSpace(word.CleanText);

    private static string BuildArticulationMessage(string archetype, string blockName, string actual, string expectation) =>
        string.Equals(expectation, TpsSpec.ArchetypeArticulationExpectations.Neutral, StringComparison.Ordinal)
            ? $"Archetype '{archetype}' expects natural diction without articulation tags, but block '{blockName}' uses '{actual}'."
            : $"Archetype '{archetype}' expects '{expectation}' articulation, but block '{blockName}' uses '{actual}'.";

    private static string BuildArticulationSuggestion(string expectation) =>
        string.Equals(expectation, TpsSpec.ArchetypeArticulationExpectations.Neutral, StringComparison.Ordinal)
            ? "Remove [legato] or [staccato] tags from this archetype scope."
            : $"Prefer [{expectation}]...[/{expectation}] when you want to reinforce this archetype.";

    private static string ResolveVolumeLabel(string? actual) =>
        string.IsNullOrWhiteSpace(actual) ? DefaultVolumeLabel : actual;

    private static string ResolveArticulationLabel(string? actual) =>
        string.IsNullOrWhiteSpace(actual) ? UnknownArticulationLabel : actual;

    private static string BuildVolumeMessage(string archetype, string blockName, string actual, string expectation)
    {
        if (string.Equals(expectation, TpsSpec.ArchetypeVolumeExpectations.DefaultOnly, StringComparison.Ordinal))
        {
            return $"Archetype '{archetype}' expects default volume, but block '{blockName}' uses '{actual}'.";
        }

        if (string.Equals(expectation, TpsSpec.ArchetypeVolumeExpectations.SoftOrDefault, StringComparison.Ordinal))
        {
            return $"Archetype '{archetype}' expects soft or default volume, but block '{blockName}' uses '{actual}'.";
        }

        return $"Archetype '{archetype}' expects loud volume, but block '{blockName}' uses '{actual}'.";
    }

    private static string BuildVolumeSuggestion(string expectation)
    {
        if (string.Equals(expectation, TpsSpec.ArchetypeVolumeExpectations.DefaultOnly, StringComparison.Ordinal))
        {
            return "Remove explicit volume tags from this archetype scope.";
        }

        if (string.Equals(expectation, TpsSpec.ArchetypeVolumeExpectations.SoftOrDefault, StringComparison.Ordinal))
        {
            return "Use [soft] sparingly or leave volume untagged in this scope.";
        }

        return "Prefer [loud] when this archetype needs an explicit volume tag.";
    }

    private static string FormatMetric(double? value) =>
        value is double numeric
            ? numeric.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)
            : "0";
}
