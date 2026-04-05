namespace ManagedCode.Tps;

public static partial class TpsSpec
{
    public static IReadOnlySet<string> WarningDiagnosticCodes { get; } =
        new HashSet<string>(StringComparer.Ordinal)
        {
            DiagnosticCodes.InvalidHeaderParameter,
            DiagnosticCodes.ArchetypeArticulationMismatch,
            DiagnosticCodes.ArchetypeEnergyMismatch,
            DiagnosticCodes.ArchetypeMelodyMismatch,
            DiagnosticCodes.ArchetypeVolumeMismatch,
            DiagnosticCodes.ArchetypeSpeedMismatch,
            DiagnosticCodes.ArchetypeRhythmPhraseLength,
            DiagnosticCodes.ArchetypeRhythmPauseFrequency,
            DiagnosticCodes.ArchetypeRhythmPauseDuration,
            DiagnosticCodes.ArchetypeRhythmEmphasisDensity,
            DiagnosticCodes.ArchetypeRhythmSpeedVariation
        };

    public static IReadOnlyList<string> Emotions { get; } =
    [
        EmotionNames.Neutral,
        EmotionNames.Warm,
        EmotionNames.Professional,
        EmotionNames.Focused,
        EmotionNames.Concerned,
        EmotionNames.Urgent,
        EmotionNames.Motivational,
        EmotionNames.Excited,
        EmotionNames.Happy,
        EmotionNames.Sad,
        EmotionNames.Calm,
        EmotionNames.Energetic
    ];

    public static IReadOnlyList<string> DeliveryModes { get; } =
    [
        Tags.Sarcasm,
        Tags.Aside,
        Tags.Rhetorical,
        Tags.Building
    ];

    public static IReadOnlyList<string> VolumeLevels { get; } =
    [
        Tags.Loud,
        Tags.Soft,
        Tags.Whisper
    ];

    public static IReadOnlyList<string> RelativeSpeedTags { get; } =
    [
        Tags.Xslow,
        Tags.Slow,
        Tags.Fast,
        Tags.Xfast,
        Tags.Normal
    ];

    public static IReadOnlyList<string> EditPointPriorities { get; } =
    [
        EditPointPriorityNames.High,
        EditPointPriorityNames.Medium,
        EditPointPriorityNames.Low
    ];

    public static IReadOnlyList<string> ArticulationStyles { get; } = [Tags.Legato, Tags.Staccato];

    public static IReadOnlyDictionary<string, int> DefaultSpeedOffsets { get; } =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [Tags.Xslow] = SpeedOffsetValues.Xslow,
            [Tags.Slow] = SpeedOffsetValues.Slow,
            [Tags.Fast] = SpeedOffsetValues.Fast,
            [Tags.Xfast] = SpeedOffsetValues.Xfast
        };

    public static IReadOnlyDictionary<string, EmotionPalette> EmotionPalettes { get; } =
        new Dictionary<string, EmotionPalette>(StringComparer.OrdinalIgnoreCase)
        {
            [EmotionNames.Neutral] = EmotionPaletteValues.Neutral,
            [EmotionNames.Warm] = EmotionPaletteValues.Warm,
            [EmotionNames.Professional] = EmotionPaletteValues.Professional,
            [EmotionNames.Focused] = EmotionPaletteValues.Focused,
            [EmotionNames.Concerned] = EmotionPaletteValues.Concerned,
            [EmotionNames.Urgent] = EmotionPaletteValues.Urgent,
            [EmotionNames.Motivational] = EmotionPaletteValues.Motivational,
            [EmotionNames.Excited] = EmotionPaletteValues.Excited,
            [EmotionNames.Happy] = EmotionPaletteValues.Happy,
            [EmotionNames.Sad] = EmotionPaletteValues.Sad,
            [EmotionNames.Calm] = EmotionPaletteValues.Calm,
            [EmotionNames.Energetic] = EmotionPaletteValues.Energetic
        };

    public static IReadOnlyDictionary<string, string> EmotionHeadCues { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [EmotionNames.Neutral] = HeadCueCodes.H0,
            [EmotionNames.Calm] = HeadCueCodes.H0,
            [EmotionNames.Professional] = HeadCueCodes.H9,
            [EmotionNames.Focused] = HeadCueCodes.H5,
            [EmotionNames.Motivational] = HeadCueCodes.H9,
            [EmotionNames.Urgent] = HeadCueCodes.H4,
            [EmotionNames.Concerned] = HeadCueCodes.H1,
            [EmotionNames.Sad] = HeadCueCodes.H1,
            [EmotionNames.Warm] = HeadCueCodes.H7,
            [EmotionNames.Happy] = HeadCueCodes.H6,
            [EmotionNames.Excited] = HeadCueCodes.H6,
            [EmotionNames.Energetic] = HeadCueCodes.H8
        };
}
