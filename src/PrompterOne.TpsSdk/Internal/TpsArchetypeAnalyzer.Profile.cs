using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal static partial class TpsArchetypeAnalyzer
{
    private static void AppendProfileWarnings(
        ArchetypeDiagnosticTarget target,
        string archetype,
        TpsArchetypeProfile profile,
        IReadOnlyList<CompiledWord> spokenWords,
        IReadOnlyList<int> lineStarts,
        List<TpsDiagnostic> diagnostics)
    {
        var articulationConflict = spokenWords.FirstOrDefault(word => IsArticulationMismatch(word.Metadata.ArticulationStyle, profile.Articulation));
        if (articulationConflict is not null)
        {
            diagnostics.Add(TpsSupport.CreateWarningDiagnostic(
                TpsSpec.DiagnosticCodes.ArchetypeArticulationMismatch,
                BuildArticulationMessage(archetype, target.Block.Name, ResolveArticulationLabel(articulationConflict.Metadata.ArticulationStyle), profile.Articulation),
                target.RangeStart,
                target.RangeEnd,
                lineStarts,
                BuildArticulationSuggestion(profile.Articulation)));
        }

        var energyConflict = spokenWords.FirstOrDefault(word => IsOutOfRange(word.Metadata.EnergyLevel, profile.Energy));
        if (energyConflict is not null && energyConflict.Metadata.EnergyLevel is int energyLevel)
        {
            diagnostics.Add(TpsSupport.CreateWarningDiagnostic(
                TpsSpec.DiagnosticCodes.ArchetypeEnergyMismatch,
                $"Archetype '{archetype}' expects energy between {profile.Energy.Min} and {profile.Energy.Max}, but block '{target.Block.Name}' uses {energyLevel} on '{energyConflict.CleanText}'.",
                target.RangeStart,
                target.RangeEnd,
                lineStarts,
                $"Keep [energy:N] between {profile.Energy.Min} and {profile.Energy.Max} for this archetype."));
        }

        var melodyConflict = spokenWords.FirstOrDefault(word => IsOutOfRange(word.Metadata.MelodyLevel, profile.Melody));
        if (melodyConflict is not null && melodyConflict.Metadata.MelodyLevel is int melodyLevel)
        {
            diagnostics.Add(TpsSupport.CreateWarningDiagnostic(
                TpsSpec.DiagnosticCodes.ArchetypeMelodyMismatch,
                $"Archetype '{archetype}' expects melody between {profile.Melody.Min} and {profile.Melody.Max}, but block '{target.Block.Name}' uses {melodyLevel} on '{melodyConflict.CleanText}'.",
                target.RangeStart,
                target.RangeEnd,
                lineStarts,
                $"Keep [melody:N] between {profile.Melody.Min} and {profile.Melody.Max} for this archetype."));
        }

        var volumeConflict = spokenWords.FirstOrDefault(word => IsVolumeMismatch(word.Metadata.VolumeLevel, profile.Volume));
        if (volumeConflict is not null)
        {
            diagnostics.Add(TpsSupport.CreateWarningDiagnostic(
                TpsSpec.DiagnosticCodes.ArchetypeVolumeMismatch,
                BuildVolumeMessage(archetype, target.Block.Name, ResolveVolumeLabel(volumeConflict.Metadata.VolumeLevel), profile.Volume),
                target.RangeStart,
                target.RangeEnd,
                lineStarts,
                BuildVolumeSuggestion(profile.Volume)));
        }

        var speedConflict = spokenWords.FirstOrDefault(word => IsSpeedMismatch(word, target.Block.TargetWpm, profile.Speed));
        if (speedConflict is not null)
        {
            var effectiveWpm = ResolveEffectiveWordWpm(speedConflict, target.Block.TargetWpm);
            diagnostics.Add(TpsSupport.CreateWarningDiagnostic(
                TpsSpec.DiagnosticCodes.ArchetypeSpeedMismatch,
                $"Archetype '{archetype}' expects inline speed changes to stay between {profile.Speed.Min} and {profile.Speed.Max} WPM, but block '{target.Block.Name}' reaches {effectiveWpm} WPM on '{speedConflict.CleanText}'.",
                target.RangeStart,
                target.RangeEnd,
                lineStarts,
                $"Prefer inline speed tags that keep this scope between {profile.Speed.Min} and {profile.Speed.Max} WPM."));
        }
    }
}
