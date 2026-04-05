using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal sealed record ArchetypeDiagnosticTarget(CompiledBlock Block, int RangeStart, int RangeEnd);

internal sealed record ArchetypeScopeMetrics(
    double AveragePhraseLength,
    double PauseFrequencyPer100Words,
    double? AveragePauseDurationMs,
    double EmphasisDensityPercent,
    double SpeedVariationPer100Words);
