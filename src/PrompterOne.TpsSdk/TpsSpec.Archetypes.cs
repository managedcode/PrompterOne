namespace ManagedCode.Tps;

public static partial class TpsSpec
{
    public static class ArchetypeNames
    {
        public const string Friend = "friend";
        public const string Motivator = "motivator";
        public const string Educator = "educator";
        public const string Coach = "coach";
        public const string Storyteller = "storyteller";
        public const string Entertainer = "entertainer";
    }

    public static IReadOnlyList<string> Archetypes { get; } =
    [
        ArchetypeNames.Friend,
        ArchetypeNames.Motivator,
        ArchetypeNames.Educator,
        ArchetypeNames.Coach,
        ArchetypeNames.Storyteller,
        ArchetypeNames.Entertainer
    ];

    public static IReadOnlyDictionary<string, int> ArchetypeRecommendedWpm { get; } =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [ArchetypeNames.Friend] = ArchetypeRecommendedWpmValues.Friend,
            [ArchetypeNames.Motivator] = ArchetypeRecommendedWpmValues.Motivator,
            [ArchetypeNames.Educator] = ArchetypeRecommendedWpmValues.Educator,
            [ArchetypeNames.Coach] = ArchetypeRecommendedWpmValues.Coach,
            [ArchetypeNames.Storyteller] = ArchetypeRecommendedWpmValues.Storyteller,
            [ArchetypeNames.Entertainer] = ArchetypeRecommendedWpmValues.Entertainer
        };

    public static class ArchetypeArticulationExpectations
    {
        public const string Flexible = "flexible";
        public const string Legato = "legato";
        public const string Neutral = "neutral";
        public const string Staccato = "staccato";
    }

    public static class ArchetypeVolumeExpectations
    {
        public const string DefaultOnly = "default-only";
        public const string Flexible = "flexible";
        public const string LoudOnly = "loud-only";
        public const string SoftOrDefault = "soft-or-default";
    }

    public static IReadOnlyDictionary<string, TpsArchetypeProfile> ArchetypeProfiles { get; } =
        new Dictionary<string, TpsArchetypeProfile>(StringComparer.OrdinalIgnoreCase)
        {
            [ArchetypeNames.Friend] = new(ArchetypeArticulationExpectations.Legato, ArchetypeRangeValues.FriendEnergy, ArchetypeRangeValues.FriendMelody, ArchetypeVolumeExpectations.SoftOrDefault, ArchetypeRangeValues.FriendSpeed),
            [ArchetypeNames.Motivator] = new(ArchetypeArticulationExpectations.Legato, ArchetypeRangeValues.MotivatorEnergy, ArchetypeRangeValues.MotivatorMelody, ArchetypeVolumeExpectations.LoudOnly, ArchetypeRangeValues.MotivatorSpeed),
            [ArchetypeNames.Educator] = new(ArchetypeArticulationExpectations.Neutral, ArchetypeRangeValues.EducatorEnergy, ArchetypeRangeValues.EducatorMelody, ArchetypeVolumeExpectations.DefaultOnly, ArchetypeRangeValues.EducatorSpeed),
            [ArchetypeNames.Coach] = new(ArchetypeArticulationExpectations.Staccato, ArchetypeRangeValues.CoachEnergy, ArchetypeRangeValues.CoachMelody, ArchetypeVolumeExpectations.LoudOnly, ArchetypeRangeValues.CoachSpeed),
            [ArchetypeNames.Storyteller] = new(ArchetypeArticulationExpectations.Flexible, ArchetypeRangeValues.StorytellerEnergy, ArchetypeRangeValues.StorytellerMelody, ArchetypeVolumeExpectations.Flexible, ArchetypeRangeValues.StorytellerSpeed),
            [ArchetypeNames.Entertainer] = new(ArchetypeArticulationExpectations.Flexible, ArchetypeRangeValues.EntertainerEnergy, ArchetypeRangeValues.EntertainerMelody, ArchetypeVolumeExpectations.Flexible, ArchetypeRangeValues.EntertainerSpeed)
        };

    public static IReadOnlyDictionary<string, TpsArchetypeRhythmProfile> ArchetypeRhythmProfiles { get; } =
        new Dictionary<string, TpsArchetypeRhythmProfile>(StringComparer.OrdinalIgnoreCase)
        {
            [ArchetypeNames.Friend] = new(ArchetypeRangeValues.FriendPhraseLength, ArchetypeRangeValues.FriendPauseFrequency, ArchetypeRangeValues.FriendPauseDuration, ArchetypeRangeValues.FriendEmphasisDensity, ArchetypeRangeValues.FriendSpeedVariation),
            [ArchetypeNames.Motivator] = new(ArchetypeRangeValues.MotivatorPhraseLength, ArchetypeRangeValues.MotivatorPauseFrequency, ArchetypeRangeValues.MotivatorPauseDuration, ArchetypeRangeValues.MotivatorEmphasisDensity, ArchetypeRangeValues.MotivatorSpeedVariation),
            [ArchetypeNames.Educator] = new(ArchetypeRangeValues.EducatorPhraseLength, ArchetypeRangeValues.EducatorPauseFrequency, ArchetypeRangeValues.EducatorPauseDuration, ArchetypeRangeValues.EducatorEmphasisDensity, ArchetypeRangeValues.EducatorSpeedVariation),
            [ArchetypeNames.Coach] = new(ArchetypeRangeValues.CoachPhraseLength, ArchetypeRangeValues.CoachPauseFrequency, ArchetypeRangeValues.CoachPauseDuration, ArchetypeRangeValues.CoachEmphasisDensity, ArchetypeRangeValues.CoachSpeedVariation),
            [ArchetypeNames.Storyteller] = new(ArchetypeRangeValues.StorytellerPhraseLength, ArchetypeRangeValues.StorytellerPauseFrequency, ArchetypeRangeValues.StorytellerPauseDuration, ArchetypeRangeValues.StorytellerEmphasisDensity, ArchetypeRangeValues.StorytellerSpeedVariation),
            [ArchetypeNames.Entertainer] = new(ArchetypeRangeValues.EntertainerPhraseLength, ArchetypeRangeValues.EntertainerPauseFrequency, ArchetypeRangeValues.EntertainerPauseDuration, ArchetypeRangeValues.EntertainerEmphasisDensity, ArchetypeRangeValues.EntertainerSpeedVariation)
        };
}
