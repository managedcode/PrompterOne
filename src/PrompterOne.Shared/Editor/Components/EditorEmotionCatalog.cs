namespace PrompterOne.Shared.Components.Editor;

public static class EditorEmotionCatalog
{
    public static IReadOnlyList<EditorEmotionOption> Options { get; } =
    [
        new("Warm", "warm"),
        new("Concerned", "concerned"),
        new("Focused", "focused"),
        new("Motivational", "motivational"),
        new("Neutral", "neutral"),
        new("Urgent", "urgent"),
        new("Happy", "happy"),
        new("Excited", "excited"),
        new("Sad", "sad"),
        new("Calm", "calm"),
        new("Energetic", "energetic"),
        new("Professional", "professional")
    ];

    public static string GetKey(string labelOrKey)
    {
        if (string.IsNullOrWhiteSpace(labelOrKey))
        {
            return string.Empty;
        }

        var normalized = labelOrKey.Trim();
        var match = Options.FirstOrDefault(option =>
            string.Equals(option.Label, normalized, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(option.Key, normalized, StringComparison.OrdinalIgnoreCase));

        return match?.Key ?? normalized.ToLowerInvariant();
    }

    public static string GetLabel(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var normalized = key.Trim();
        var match = Options.FirstOrDefault(option =>
            string.Equals(option.Key, normalized, StringComparison.OrdinalIgnoreCase));

        return match?.Label ?? normalized;
    }
}

public sealed record EditorEmotionOption(string Label, string Key);
