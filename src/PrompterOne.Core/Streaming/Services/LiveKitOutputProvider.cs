using PrompterOne.Core.Abstractions;
using PrompterOne.Core.Models.Streaming;

namespace PrompterOne.Core.Services.Streaming;

public sealed class LiveKitOutputProvider : IStreamingOutputProvider
{
    public string Id => "livekit";

    public StreamingProviderKind Kind => StreamingProviderKind.LiveKit;

    public string DisplayName => "LiveKit";

    public StreamingPublishDescriptor Describe(StreamingProfile profile)
    {
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["serverUrl"] = profile.ServerUrl ?? string.Empty,
            ["roomName"] = profile.RoomName ?? string.Empty,
            ["token"] = string.IsNullOrWhiteSpace(profile.Token) ? string.Empty : "configured"
        };

        var isReady = !string.IsNullOrWhiteSpace(profile.ServerUrl)
            && !string.IsNullOrWhiteSpace(profile.RoomName)
            && !string.IsNullOrWhiteSpace(profile.Token);

        return new StreamingPublishDescriptor(
            ProviderId: Id,
            ProviderKind: Kind,
            DisplayName: DisplayName,
            IsReady: isReady,
            RequiresExternalRelay: false,
            Summary: isReady
                ? "Publishes the composed browser scene into a LiveKit room."
                : "Configure the LiveKit server URL, room name, and access token.",
            Parameters: parameters);
    }
}
