namespace PrompterLive.Core.Models.Streaming;

public enum StreamingProviderKind
{
    LiveKit,
    VdoNinja,
    Rtmp
}

public sealed record StreamingDestination(
    string Name,
    string Url,
    string? StreamKey = null,
    bool IsEnabled = true);

public sealed record StreamingProfile(
    string Id,
    string Name,
    StreamingProviderKind ProviderKind,
    string? ServerUrl = null,
    string? RoomName = null,
    string? Token = null,
    string? PublishUrl = null,
    IReadOnlyList<StreamingDestination>? Destinations = null,
    bool MirrorLocalPreview = true)
{
    public static StreamingProfile CreateDefault(StreamingProviderKind providerKind) =>
        new(
            Id: providerKind.ToString().ToLowerInvariant(),
            Name: providerKind switch
            {
                StreamingProviderKind.LiveKit => "LiveKit Stage",
                StreamingProviderKind.VdoNinja => "VDO.Ninja Room",
                _ => "RTMP Relay"
            },
            ProviderKind: providerKind,
            Destinations: Array.Empty<StreamingDestination>());
}

public sealed record StreamingPublishDescriptor(
    string ProviderId,
    StreamingProviderKind ProviderKind,
    string DisplayName,
    bool IsReady,
    bool RequiresExternalRelay,
    string Summary,
    IReadOnlyDictionary<string, string> Parameters,
    string? LaunchUrl = null);
