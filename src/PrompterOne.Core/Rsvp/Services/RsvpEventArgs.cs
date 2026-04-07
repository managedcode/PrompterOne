using PrompterOne.Core.Models.HeadCues;

namespace PrompterOne.Core.Services.Rsvp;

public class WordDisplayEventArgs : EventArgs
{
    public string PreORP { get; set; } = string.Empty;
    public string OrpChar { get; set; } = string.Empty;
    public string PostORP { get; set; } = string.Empty;
    public string LeftWord1 { get; set; } = string.Empty;
    public string LeftWord2 { get; set; } = string.Empty;
    public string LeftWord3 { get; set; } = string.Empty;
    public string LeftWord4 { get; set; } = string.Empty;
    public string LeftWord5 { get; set; } = string.Empty;
    public string RightWord1 { get; set; } = string.Empty;
    public string RightWord2 { get; set; } = string.Empty;
    public string RightWord3 { get; set; } = string.Empty;
    public string RightWord4 { get; set; } = string.Empty;
    public string RightWord5 { get; set; } = string.Empty;
    public int CurrentWordIndex { get; set; }
    public int TotalWords { get; set; }
    public string EmotionName { get; set; } = string.Empty;
    public IReadOnlyList<string> PhraseWords { get; set; } = Array.Empty<string>();
    public int PhraseEstimatedDurationMs { get; set; }
    public int PhrasePauseAfterMs { get; set; }
    public bool PhraseContainsPauseCue { get; set; }
    public bool IsPause { get; set; }
    public string UpcomingEmotionName { get; set; } = string.Empty;
    public string HeadCueId { get; set; } = HeadCueCatalog.DefaultCueId;
}

public class PlaybackStateEventArgs : EventArgs
{
    public bool IsPlaying { get; set; }
    public bool IsStopped { get; set; }
}

public class ProgressEventArgs : EventArgs
{
    public int CurrentWordIndex { get; set; }
    public int TotalWords { get; set; }
    public double ProgressPercentage { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public TimeSpan TimeElapsed { get; set; }
}

public class EmotionChangeEventArgs : EventArgs
{
    public string EmotionName { get; set; } = string.Empty;
}
