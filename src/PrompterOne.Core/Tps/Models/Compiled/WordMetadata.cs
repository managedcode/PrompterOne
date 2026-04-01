namespace PrompterOne.Core.Models.CompiledScript;

public class WordMetadata
{
    public bool IsEmphasis { get; set; }
    public bool IsPause { get; set; }
    public int? PauseDuration { get; set; }
    public string? Color { get; set; } // Direct color from TPS tags (red, green, yellow, etc.)
    public string? EmotionHint { get; set; }
    public string? InlineEmotionHint { get; set; }
    public string? PronunciationGuide { get; set; }
    public int? SpeedOverride { get; set; }
    public float? SpeedMultiplier { get; set; }
    public string? HeadCue { get; set; }
}
