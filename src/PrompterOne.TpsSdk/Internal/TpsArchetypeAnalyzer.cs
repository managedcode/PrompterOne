using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal static partial class TpsArchetypeAnalyzer
{
    public static void AppendDiagnostics(IEnumerable<ArchetypeDiagnosticTarget> targets, IReadOnlyList<int> lineStarts, List<TpsDiagnostic> diagnostics)
    {
        foreach (var target in targets)
        {
            var archetype = target.Block.Archetype?.ToLowerInvariant();
            if (archetype is null ||
                !TpsSpec.ArchetypeProfiles.TryGetValue(archetype, out var profile) ||
                !TpsSpec.ArchetypeRhythmProfiles.TryGetValue(archetype, out var rhythm))
            {
                continue;
            }

            var spokenWords = target.Block.Words.Where(IsSpokenWord).ToArray();
            if (spokenWords.Length == 0)
            {
                continue;
            }

            AppendProfileWarnings(target, archetype, profile, spokenWords, lineStarts, diagnostics);
            AppendRhythmWarnings(target, archetype, rhythm, spokenWords, lineStarts, diagnostics);
        }
    }
}
