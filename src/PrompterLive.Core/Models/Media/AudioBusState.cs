namespace PrompterLive.Core.Models.Media;

public enum AudioRouteTarget
{
    Monitor,
    Stream,
    Both
}

public sealed record AudioInputState(
    string DeviceId,
    string Label,
    int DelayMs = 0,
    double Gain = 1.0,
    bool IsMuted = false,
    AudioRouteTarget RouteTarget = AudioRouteTarget.Both);

public sealed record AudioBusState(
    IReadOnlyList<AudioInputState> Inputs,
    double MasterGain = 1.0,
    bool MonitorEnabled = true)
{
    public static AudioBusState Empty { get; } = new(Array.Empty<AudioInputState>());
}
