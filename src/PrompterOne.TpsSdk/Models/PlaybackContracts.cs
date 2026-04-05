using System.Text.Json.Serialization;

namespace ManagedCode.Tps.Models;

[JsonConverter(typeof(TpsPlaybackStatusJsonConverter))]
public enum TpsPlaybackStatus
{
    Idle,
    Playing,
    Paused,
    Completed
}

public static class TpsPlaybackDefaults
{
    public const int DefaultSpeedStepWpm = 10;
    public const int DefaultTickIntervalMs = 16;
    public const int MinimumSpeedStepWpm = 1;
    public const int MinimumTickIntervalMs = 1;
}

public static class TpsPlaybackEventNames
{
    public const string StateChanged = "stateChanged";
    public const string WordChanged = "wordChanged";
    public const string PhraseChanged = "phraseChanged";
    public const string BlockChanged = "blockChanged";
    public const string SegmentChanged = "segmentChanged";
    public const string StatusChanged = "statusChanged";
    public const string Completed = "completed";
    public const string SnapshotChanged = "snapshotChanged";
    public const string ObserveSnapshot = "observeSnapshot";
}

public class TpsPlaybackSessionOptions
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("baseWpm")]
    public int? BaseWpm { get; init; }

    [JsonIgnore]
    public SynchronizationContext? EventSynchronizationContext { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("initialSpeedOffsetWpm")]
    public int? InitialSpeedOffsetWpm { get; init; }

    [JsonPropertyName("speedStepWpm")]
    public int SpeedStepWpm
    {
        get;
        init => field = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "SpeedStepWpm must be greater than zero.");
    } = TpsPlaybackDefaults.DefaultSpeedStepWpm;

    [JsonPropertyName("tickIntervalMs")]
    public int TickIntervalMs
    {
        get;
        init => field = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value), "TickIntervalMs must be greater than zero.");
    } = TpsPlaybackDefaults.DefaultTickIntervalMs;

    [JsonIgnore]
    public TimeProvider TimeProvider
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    } = TimeProvider.System;
}

public sealed class TpsStandalonePlayerOptions : TpsPlaybackSessionOptions
{
    [JsonPropertyName("autoPlay")]
    public bool AutoPlay { get; init; }
}

public sealed record TpsPlaybackStateChangedEventArgs(
    [property: JsonPropertyName("state")] PlayerState State,
    [property: JsonPropertyName("previousState")] PlayerState PreviousState,
    [property: JsonPropertyName("status")] TpsPlaybackStatus Status);

public sealed record TpsPlaybackStatusChangedEventArgs(
    [property: JsonPropertyName("state")] PlayerState State,
    [property: JsonPropertyName("previousStatus")] TpsPlaybackStatus PreviousStatus,
    [property: JsonPropertyName("status")] TpsPlaybackStatus Status);

public sealed record TpsPlaybackTempo(
    [property: JsonPropertyName("baseWpm")] int BaseWpm,
    [property: JsonPropertyName("effectiveBaseWpm")] int EffectiveBaseWpm,
    [property: JsonPropertyName("speedOffsetWpm")] int SpeedOffsetWpm,
    [property: JsonPropertyName("speedStepWpm")] int SpeedStepWpm,
    [property: JsonPropertyName("playbackRate")] double PlaybackRate);

public sealed record TpsPlaybackControls(
    [property: JsonPropertyName("canPlay")] bool CanPlay,
    [property: JsonPropertyName("canPause")] bool CanPause,
    [property: JsonPropertyName("canStop")] bool CanStop,
    [property: JsonPropertyName("canNextWord")] bool CanNextWord,
    [property: JsonPropertyName("canPreviousWord")] bool CanPreviousWord,
    [property: JsonPropertyName("canNextBlock")] bool CanNextBlock,
    [property: JsonPropertyName("canPreviousBlock")] bool CanPreviousBlock,
    [property: JsonPropertyName("canIncreaseSpeed")] bool CanIncreaseSpeed,
    [property: JsonPropertyName("canDecreaseSpeed")] bool CanDecreaseSpeed);

public sealed class TpsPlaybackWordView
{
    [JsonPropertyName("word")]
    public required CompiledWord Word { get; init; }

    [JsonPropertyName("isActive")]
    public required bool IsActive { get; init; }

    [JsonPropertyName("isRead")]
    public required bool IsRead { get; init; }

    [JsonPropertyName("isUpcoming")]
    public required bool IsUpcoming { get; init; }

    [JsonPropertyName("emotion")]
    public required string Emotion { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speaker")]
    public string? Speaker { get; init; }

    [JsonPropertyName("emphasisLevel")]
    public required int EmphasisLevel { get; init; }

    [JsonPropertyName("isHighlighted")]
    public required bool IsHighlighted { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("deliveryMode")]
    public string? DeliveryMode { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("volumeLevel")]
    public string? VolumeLevel { get; init; }
}

public sealed class TpsPlaybackSnapshot
{
    [JsonPropertyName("status")]
    public required TpsPlaybackStatus Status { get; init; }

    [JsonPropertyName("state")]
    public required PlayerState State { get; init; }

    [JsonPropertyName("tempo")]
    public required TpsPlaybackTempo Tempo { get; init; }

    [JsonPropertyName("controls")]
    public required TpsPlaybackControls Controls { get; init; }

    [JsonPropertyName("visibleWords")]
    public required IReadOnlyList<TpsPlaybackWordView> VisibleWords { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("focusedWord")]
    public TpsPlaybackWordView? FocusedWord { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentWordDurationMs")]
    public int? CurrentWordDurationMs { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentWordRemainingMs")]
    public int? CurrentWordRemainingMs { get; init; }

    [JsonPropertyName("currentSegmentIndex")]
    public required int CurrentSegmentIndex { get; init; }

    [JsonPropertyName("currentBlockIndex")]
    public required int CurrentBlockIndex { get; init; }
}

public sealed record TpsPlaybackSnapshotChangedEventArgs(
    [property: JsonPropertyName("snapshot")] TpsPlaybackSnapshot Snapshot);

public sealed record TpsPlaybackListenerExceptionEventArgs(
    [property: JsonIgnore] Exception Exception,
    [property: JsonPropertyName("eventName")] string EventName,
    [property: JsonPropertyName("snapshot")] TpsPlaybackSnapshot Snapshot,
    [property: JsonPropertyName("message")] string Message);
