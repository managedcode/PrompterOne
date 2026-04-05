namespace ManagedCode.Tps;

public sealed record EmotionPalette(string Accent, string Text, string Background);

public sealed record NumericRange(int Min, int Max);

public sealed record TpsArchetypeProfile(
    string Articulation,
    NumericRange Energy,
    NumericRange Melody,
    string Volume,
    NumericRange Speed);

public sealed record TpsArchetypeRhythmProfile(
    NumericRange PhraseLength,
    NumericRange PauseFrequencyPer100Words,
    NumericRange AveragePauseDurationMs,
    NumericRange EmphasisDensityPercent,
    NumericRange SpeedVariationPer100Words);
