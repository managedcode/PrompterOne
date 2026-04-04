namespace PrompterOne.Core.Services;

internal static class TpsSpec
{
    public const int DefaultBaseWpm = 140;
    public const int MaximumWpm = 220;
    public const int MinimumWpm = 80;
    public const int MediumPauseDurationMs = 600;
    public const int ShortPauseDurationMs = 300;
    public const string DefaultEmotion = "neutral";
    public const string DefaultImplicitSegmentName = "Content";
    public const string DefaultProfile = "Actor";
    public const int DefaultFastOffset = 25;
    public const int DefaultSlowOffset = -20;
    public const int DefaultXfastOffset = 50;
    public const int DefaultXslowOffset = -40;
    public const string SpeakerPrefix = "Speaker:";
    public const string WpmSuffix = "WPM";

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
        public const string Fast = "fast";
        public const string Highlight = "highlight";
        public const string Loud = "loud";
        public const string Normal = "normal";
        public const string Pause = "pause";
        public const string Phonetic = "phonetic";
        public const string Pronunciation = "pronunciation";
        public const string Rhetorical = "rhetorical";
        public const string Sarcasm = "sarcasm";
        public const string Slow = "slow";
        public const string Soft = "soft";
        public const string Stress = "stress";
        public const string Whisper = "whisper";
        public const string Xfast = "xfast";
        public const string Xslow = "xslow";
    }

    public static class DiagnosticCodes
    {
        public const string InvalidFrontMatter = "invalid-front-matter";
        public const string InvalidHeader = "invalid-header";
        public const string InvalidHeaderParameter = "invalid-header-parameter";
        public const string InvalidPause = "invalid-pause";
        public const string InvalidTagArgument = "invalid-tag-argument";
        public const string InvalidWpm = "invalid-wpm";
        public const string MismatchedClosingTag = "mismatched-closing-tag";
        public const string UnclosedTag = "unclosed-tag";
        public const string UnknownTag = "unknown-tag";
        public const string UnterminatedTag = "unterminated-tag";
    }

    public static IReadOnlySet<string> Emotions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "neutral",
        "warm",
        "professional",
        "focused",
        "concerned",
        "urgent",
        "motivational",
        "excited",
        "happy",
        "sad",
        "calm",
        "energetic"
    };

    public static IReadOnlySet<string> DeliveryModes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Tags.Sarcasm,
        Tags.Aside,
        Tags.Rhetorical,
        Tags.Building
    };

    public static IReadOnlySet<string> VolumeLevels { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Tags.Loud,
        Tags.Soft,
        Tags.Whisper
    };

    public static IReadOnlySet<string> RelativeSpeedTags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Tags.Xslow,
        Tags.Slow,
        Tags.Fast,
        Tags.Xfast,
        Tags.Normal
    };

    public static IReadOnlySet<string> EditPointPriorities { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "high",
        "medium",
        "low"
    };

    public static IReadOnlyDictionary<string, int> DefaultSpeedOffsets { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        [Tags.Xslow] = DefaultXslowOffset,
        [Tags.Slow] = DefaultSlowOffset,
        [Tags.Fast] = DefaultFastOffset,
        [Tags.Xfast] = DefaultXfastOffset
    };

    public static IReadOnlyDictionary<string, EmotionPalette> EmotionPalettes { get; } = new Dictionary<string, EmotionPalette>(StringComparer.OrdinalIgnoreCase)
    {
        [DefaultEmotion] = new("#2563EB", "#0F172A", "#60A5FA"),
        ["warm"] = new("#EA580C", "#1C1917", "#FDBA74"),
        ["professional"] = new("#1D4ED8", "#0F172A", "#93C5FD"),
        ["focused"] = new("#15803D", "#052E16", "#86EFAC"),
        ["concerned"] = new("#B91C1C", "#1F1111", "#FCA5A5"),
        ["urgent"] = new("#DC2626", "#FFF7F7", "#FCA5A5"),
        ["motivational"] = new("#7C3AED", "#FFFFFF", "#C4B5FD"),
        ["excited"] = new("#DB2777", "#FFF7FB", "#F9A8D4"),
        ["happy"] = new("#D97706", "#1C1917", "#FCD34D"),
        ["sad"] = new("#4F46E5", "#EEF2FF", "#A5B4FC"),
        ["calm"] = new("#0F766E", "#F0FDFA", "#99F6E4"),
        ["energetic"] = new("#C2410C", "#FFF7ED", "#FDBA74")
    };

    public static IReadOnlyDictionary<string, string> EmotionHeadCues { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [DefaultEmotion] = "H0",
        ["calm"] = "H0",
        ["professional"] = "H9",
        ["focused"] = "H5",
        ["motivational"] = "H9",
        ["urgent"] = "H4",
        ["concerned"] = "H1",
        ["sad"] = "H1",
        ["warm"] = "H7",
        ["happy"] = "H6",
        ["excited"] = "H6",
        ["energetic"] = "H8"
    };
}

internal sealed record EmotionPalette(string Accent, string Text, string Background);
