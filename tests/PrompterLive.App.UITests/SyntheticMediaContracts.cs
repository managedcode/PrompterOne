namespace PrompterLive.App.UITests;

internal sealed class SyntheticMediaDeviceState
{
    public string DeviceId { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}

internal sealed class SyntheticMediaRequestState
{
    public int RequestId { get; set; }

    public bool HasVideo { get; set; }

    public bool HasAudio { get; set; }

    public string? RequestedVideoDeviceId { get; set; }

    public string? RequestedAudioDeviceId { get; set; }

    public string? ResolvedVideoDeviceId { get; set; }

    public string? ResolvedAudioDeviceId { get; set; }
}

internal sealed class SyntheticMediaMetadata
{
    public bool IsSynthetic { get; set; }

    public string? VideoDeviceId { get; set; }

    public string? AudioDeviceId { get; set; }

    public string? VideoLabel { get; set; }

    public string? AudioLabel { get; set; }
}

internal sealed class SyntheticMediaElementState
{
    public bool HasElement { get; set; }

    public bool HasStream { get; set; }

    public bool Paused { get; set; }

    public int ReadyState { get; set; }

    public int VideoTrackCount { get; set; }

    public int AudioTrackCount { get; set; }

    public SyntheticMediaMetadata? Metadata { get; set; }
}
