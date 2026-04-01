using System.Globalization;

namespace PrompterOne.Core.Localization;

public static class AppText
{
    private static readonly IReadOnlyDictionary<string, string> Strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Parser.Segment.Content"] = "Content",
        ["Parser.Title.Untitled"] = "Untitled Script",
        ["Parser.Block.Indexed"] = "Block {0}",
        ["Parser.Block.Default"] = "Block",
        ["Parser.Empty.Title"] = "Empty Script",
        ["Parser.Empty.MainContent"] = "Main Content",
        ["HeadCue.Title.H0"] = "Neutral",
        ["HeadCue.Description.H0"] = "Keep your head level and centered.",
        ["HeadCue.Title.H1"] = "Concerned",
        ["HeadCue.Description.H1"] = "Tilt slightly to signal concern.",
        ["HeadCue.Title.H2"] = "Assertive",
        ["HeadCue.Description.H2"] = "Lean in with a stronger forward intent.",
        ["HeadCue.Title.H3"] = "Reflective",
        ["HeadCue.Description.H3"] = "Softer posture for reflective beats.",
        ["HeadCue.Title.H4"] = "Urgent",
        ["HeadCue.Description.H4"] = "Sharper angle to increase urgency.",
        ["HeadCue.Title.H5"] = "Focused",
        ["HeadCue.Description.H5"] = "Bring the eyes slightly down to focus.",
        ["HeadCue.Title.H6"] = "Upbeat",
        ["HeadCue.Description.H6"] = "Lift the chin slightly for energy.",
        ["HeadCue.Title.H7"] = "Warm",
        ["HeadCue.Description.H7"] = "Relax into a friendly, welcoming pose.",
        ["HeadCue.Title.H8"] = "Energetic",
        ["HeadCue.Description.H8"] = "Open posture with a touch more motion.",
        ["HeadCue.Title.H9"] = "Professional",
        ["HeadCue.Description.H9"] = "Stable presenter posture with confidence."
    };

    public static string Get(string key) =>
        Strings.TryGetValue(key, out var value)
            ? value
            : key;

    public static string Format(string key, params object[] args) =>
        string.Format(CultureInfo.InvariantCulture, Get(key), args);

    public static string Emotion(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return "Neutral";
        }

        return key.Trim().ToLowerInvariant() switch
        {
            "warm" => "Warm",
            "concerned" => "Concerned",
            "focused" => "Focused",
            "motivational" => "Motivational",
            "urgent" => "Urgent",
            "happy" => "Happy",
            "excited" => "Excited",
            "sad" => "Sad",
            "calm" => "Calm",
            "energetic" => "Energetic",
            "professional" => "Professional",
            _ => "Neutral"
        };
    }
}
