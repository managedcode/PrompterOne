using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal static partial class TpsArchetypeAnalyzer
{
    private static void AppendRhythmWarnings(
        ArchetypeDiagnosticTarget target,
        string archetype,
        TpsArchetypeRhythmProfile rhythm,
        IReadOnlyList<CompiledWord> spokenWords,
        IReadOnlyList<int> lineStarts,
        List<TpsDiagnostic> diagnostics)
    {
        if (spokenWords.Count < TpsSpec.ArchetypeRhythmMinimumWords)
        {
            return;
        }

        var phraseWordCounts = target.Block.Phrases
            .Select(phrase => phrase.Words.Count(IsSpokenWord))
            .Where(count => count > 0)
            .ToArray();
        if (phraseWordCounts.Length < 2)
        {
            return;
        }

        var metrics = CollectScopeMetrics(target.Block, spokenWords, phraseWordCounts);

        PushRhythmWarning(
            diagnostics,
            TpsSpec.DiagnosticCodes.ArchetypeRhythmPhraseLength,
            target,
            lineStarts,
            !IsWithinRange(metrics.AveragePhraseLength, rhythm.PhraseLength),
            $"Archetype '{archetype}' expects average phrase length between {rhythm.PhraseLength.Min} and {rhythm.PhraseLength.Max} words, but block '{target.Block.Name}' averages {FormatMetric(metrics.AveragePhraseLength)}.",
            $"Break phrases so this scope averages between {rhythm.PhraseLength.Min} and {rhythm.PhraseLength.Max} words.");

        PushRhythmWarning(
            diagnostics,
            TpsSpec.DiagnosticCodes.ArchetypeRhythmPauseFrequency,
            target,
            lineStarts,
            !IsWithinRange(metrics.PauseFrequencyPer100Words, rhythm.PauseFrequencyPer100Words),
            $"Archetype '{archetype}' expects {rhythm.PauseFrequencyPer100Words.Min} to {rhythm.PauseFrequencyPer100Words.Max} pauses per 100 words, but block '{target.Block.Name}' has {FormatMetric(metrics.PauseFrequencyPer100Words)}.",
            $"Adjust pause markers so this scope lands between {rhythm.PauseFrequencyPer100Words.Min} and {rhythm.PauseFrequencyPer100Words.Max} pauses per 100 words.");

        PushRhythmWarning(
            diagnostics,
            TpsSpec.DiagnosticCodes.ArchetypeRhythmPauseDuration,
            target,
            lineStarts,
            metrics.AveragePauseDurationMs is double averagePauseDuration &&
            !IsWithinRange(averagePauseDuration, rhythm.AveragePauseDurationMs),
            $"Archetype '{archetype}' expects average pause duration between {rhythm.AveragePauseDurationMs.Min} and {rhythm.AveragePauseDurationMs.Max} ms, but block '{target.Block.Name}' averages {FormatMetric(metrics.AveragePauseDurationMs)} ms.",
            $"Adjust explicit pauses so this scope averages between {rhythm.AveragePauseDurationMs.Min} and {rhythm.AveragePauseDurationMs.Max} ms.");

        PushRhythmWarning(
            diagnostics,
            TpsSpec.DiagnosticCodes.ArchetypeRhythmEmphasisDensity,
            target,
            lineStarts,
            !IsWithinRange(metrics.EmphasisDensityPercent, rhythm.EmphasisDensityPercent),
            $"Archetype '{archetype}' expects emphasis density between {rhythm.EmphasisDensityPercent.Min}% and {rhythm.EmphasisDensityPercent.Max}%, but block '{target.Block.Name}' is {FormatMetric(metrics.EmphasisDensityPercent)}%.",
            $"Add or remove emphasis so this scope lands between {rhythm.EmphasisDensityPercent.Min}% and {rhythm.EmphasisDensityPercent.Max}%.");

        PushRhythmWarning(
            diagnostics,
            TpsSpec.DiagnosticCodes.ArchetypeRhythmSpeedVariation,
            target,
            lineStarts,
            !IsWithinRange(metrics.SpeedVariationPer100Words, rhythm.SpeedVariationPer100Words),
            $"Archetype '{archetype}' expects {rhythm.SpeedVariationPer100Words.Min} to {rhythm.SpeedVariationPer100Words.Max} inline speed changes per 100 words, but block '{target.Block.Name}' has {FormatMetric(metrics.SpeedVariationPer100Words)}.",
            $"Adjust inline speed tags so this scope lands between {rhythm.SpeedVariationPer100Words.Min} and {rhythm.SpeedVariationPer100Words.Max} changes per 100 words.");
    }

    private static ArchetypeScopeMetrics CollectScopeMetrics(CompiledBlock block, IReadOnlyList<CompiledWord> spokenWords, IReadOnlyList<int> phraseWordCounts)
    {
        var pauses = block.Words.Where(word => word.Kind == TpsSpec.WordKinds.Pause).ToArray();
        var speedVariationRuns = 0;
        var inVariation = false;

        foreach (var word in spokenWords)
        {
            var varied = HasInlineSpeedVariation(word, block.TargetWpm);
            if (varied && !inVariation)
            {
                speedVariationRuns++;
            }

            inVariation = varied;
        }

        return new ArchetypeScopeMetrics(
            AveragePhraseLength: phraseWordCounts.Average(),
            PauseFrequencyPer100Words: pauses.Length / (double)spokenWords.Count * 100d,
            AveragePauseDurationMs: pauses.Length > 0 ? pauses.Average(word => word.DisplayDurationMs) : null,
            EmphasisDensityPercent: spokenWords.Count(word => word.Metadata.IsEmphasis) / (double)spokenWords.Count * 100d,
            SpeedVariationPer100Words: speedVariationRuns / (double)spokenWords.Count * 100d);
    }

    private static void PushRhythmWarning(
        List<TpsDiagnostic> diagnostics,
        string code,
        ArchetypeDiagnosticTarget target,
        IReadOnlyList<int> lineStarts,
        bool condition,
        string message,
        string suggestion)
    {
        if (!condition)
        {
            return;
        }

        diagnostics.Add(TpsSupport.CreateWarningDiagnostic(code, message, target.RangeStart, target.RangeEnd, lineStarts, suggestion));
    }
}
