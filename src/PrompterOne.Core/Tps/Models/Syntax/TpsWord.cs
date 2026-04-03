namespace PrompterOne.Core.Models.Tps;

public class TpsWord
{
    public string Text { get; set; } = string.Empty;
    public bool IsEmphasis { get; set; }
    public int EmphasisLevel { get; set; }
    public bool IsHighlight { get; set; }
    public bool IsBreath { get; set; }
    public bool IsEditPoint { get; set; }
    public string? EditPointPriority { get; set; }
    public string? Emotion { get; set; }
    public string? VolumeLevel { get; set; }
    public string? DeliveryMode { get; set; }
    public string? PronunciationGuide { get; set; }
    public string? StressText { get; set; }
    public string? StressGuide { get; set; }
    public int? SpeedOverride { get; set; }
    public float? SpeedMultiplier { get; set; }
    public string? Speaker { get; set; }
    public int? PauseDuration { get; set; }
}
