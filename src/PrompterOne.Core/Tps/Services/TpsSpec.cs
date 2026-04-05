using SdkTpsSpec = ManagedCode.Tps.TpsSpec;

namespace PrompterOne.Core.Services;

internal static class TpsSpec
{
    public const int DefaultBaseWpm = SdkTpsSpec.DefaultBaseWpm;
    public const int MaximumWpm = SdkTpsSpec.MaximumWpm;
    public const int MinimumWpm = SdkTpsSpec.MinimumWpm;
    public const int MediumPauseDurationMs = SdkTpsSpec.MediumPauseDurationMs;
    public const int ShortPauseDurationMs = SdkTpsSpec.ShortPauseDurationMs;
    public const string DefaultEmotion = SdkTpsSpec.DefaultEmotion;
    public const string DefaultImplicitSegmentName = "Content";
    public const string DefaultProfile = "Actor";
    public const int DefaultFastOffset = SdkTpsSpec.SpeedOffsetValues.Fast;
    public const int DefaultSlowOffset = SdkTpsSpec.SpeedOffsetValues.Slow;
    public const int DefaultXfastOffset = SdkTpsSpec.SpeedOffsetValues.Xfast;
    public const int DefaultXslowOffset = SdkTpsSpec.SpeedOffsetValues.Xslow;
    public const string ArchetypePrefix = SdkTpsSpec.ArchetypePrefix;
    public const string SpeakerPrefix = SdkTpsSpec.SpeakerPrefix;
    public const string WpmSuffix = SdkTpsSpec.WpmSuffix;
    public const int EnergyLevelMin = 1;
    public const int EnergyLevelMax = 10;
    public const int MelodyLevelMin = 1;
    public const int MelodyLevelMax = 10;

    public static class FrontMatterKeys
    {
        public const string Author = "author";
        public const string BaseWpm = "base_wpm";
        public const string Created = "created";
        public const string Duration = "duration";
        public const string Profile = "profile";
        public const string SpeedOffsetsFast = "speed_offsets.fast";
        public const string SpeedOffsetsSlow = "speed_offsets.slow";
        public const string SpeedOffsetsXfast = "speed_offsets.xfast";
        public const string SpeedOffsetsXslow = "speed_offsets.xslow";
        public const string Title = "title";
        public const string Version = "version";
    }

    public static class LegacyKeys
    {
        public const string DisplayDuration = "display_duration";
        public const string FastOffset = "fast_offset";
        public const string PresetsFast = "presets.fast";
        public const string PresetsSlow = "presets.slow";
        public const string PresetsXfast = "presets.xfast";
        public const string PresetsXslow = "presets.xslow";
        public const string SlowOffset = "slow_offset";
        public const string XfastOffset = "xfast_offset";
        public const string XslowOffset = "xslow_offset";
    }

    public static class Tags
    {
        public const string Aside = "aside";
        public const string Breath = "breath";
        public const string Building = "building";
        public const string EditPoint = "edit_point";
        public const string Emphasis = "emphasis";
        public const string Energy = "energy";
        public const string Fast = "fast";
        public const string Highlight = "highlight";
        public const string Legato = "legato";
        public const string Loud = "loud";
        public const string Melody = "melody";
        public const string Normal = "normal";
        public const string Pause = "pause";
        public const string Phonetic = "phonetic";
        public const string Pronunciation = "pronunciation";
        public const string Rhetorical = "rhetorical";
        public const string Sarcasm = "sarcasm";
        public const string Slow = "slow";
        public const string Soft = "soft";
        public const string Staccato = "staccato";
        public const string Stress = "stress";
        public const string Whisper = "whisper";
        public const string Xfast = "xfast";
        public const string Xslow = "xslow";
    }

    public static class DiagnosticCodes
    {
        public const string InvalidEnergyLevel = "invalid-energy-level";
        public const string InvalidFrontMatter = "invalid-front-matter";
        public const string InvalidHeader = "invalid-header";
        public const string InvalidHeaderParameter = "invalid-header-parameter";
        public const string InvalidMelodyLevel = "invalid-melody-level";
        public const string InvalidPause = "invalid-pause";
        public const string InvalidTagArgument = "invalid-tag-argument";
        public const string InvalidWpm = "invalid-wpm";
        public const string MismatchedClosingTag = "mismatched-closing-tag";
        public const string UnclosedTag = "unclosed-tag";
        public const string UnknownArchetype = "unknown-archetype";
        public const string UnknownTag = "unknown-tag";
        public const string UnterminatedTag = "unterminated-tag";
    }

    public static IReadOnlySet<string> Emotions { get; } =
        new HashSet<string>(SdkTpsSpec.Emotions, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> DeliveryModes { get; } =
        new HashSet<string>(SdkTpsSpec.DeliveryModes, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> VolumeLevels { get; } =
        new HashSet<string>(SdkTpsSpec.VolumeLevels, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> RelativeSpeedTags { get; } =
        new HashSet<string>(SdkTpsSpec.RelativeSpeedTags, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlySet<string> EditPointPriorities { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "high",
        "medium",
        "low"
    };

    public static IReadOnlySet<string> ArticulationStyles { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Tags.Legato,
        Tags.Staccato
    };

    public static class ArchetypeNames
    {
        public const string Friend = "friend";
        public const string Motivator = "motivator";
        public const string Educator = "educator";
        public const string Coach = "coach";
        public const string Storyteller = "storyteller";
        public const string Entertainer = "entertainer";
    }

    public static IReadOnlySet<string> Archetypes { get; } =
        new HashSet<string>(SdkTpsSpec.Archetypes, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, int> ArchetypeRecommendedWpm { get; } =
        new Dictionary<string, int>(SdkTpsSpec.ArchetypeRecommendedWpm, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, int> DefaultSpeedOffsets { get; } =
        new Dictionary<string, int>(SdkTpsSpec.DefaultSpeedOffsets, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, EmotionPalette> EmotionPalettes { get; } =
        SdkTpsSpec.EmotionPalettes.ToDictionary(
            pair => pair.Key,
            pair => new EmotionPalette(pair.Value.Accent, pair.Value.Text, pair.Value.Background),
            StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, string> EmotionHeadCues { get; } =
        new Dictionary<string, string>(SdkTpsSpec.EmotionHeadCues, StringComparer.OrdinalIgnoreCase);
}

internal sealed record EmotionPalette(string Accent, string Text, string Background);
