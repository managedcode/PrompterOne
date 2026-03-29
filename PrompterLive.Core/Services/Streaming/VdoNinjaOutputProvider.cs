using PrompterLive.Core.Abstractions;
using PrompterLive.Core.Models.Streaming;

namespace PrompterLive.Core.Services.Streaming;

public sealed class VdoNinjaOutputProvider : IStreamingOutputProvider
{
    public string Id => "vdoninja";

    public StreamingProviderKind Kind => StreamingProviderKind.VdoNinja;

    public string DisplayName => "VDO.Ninja";

    public StreamingPublishDescriptor Describe(StreamingProfile profile)
    {
        var launchUrl = profile.PublishUrl;
        if (string.IsNullOrWhiteSpace(launchUrl) && !string.IsNullOrWhiteSpace(profile.RoomName))
        {
            launchUrl = $"https://vdo.ninja/?room={Uri.EscapeDataString(profile.RoomName)}&push={Uri.EscapeDataString(profile.RoomName)}";
        }

        var parameters = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["roomName"] = profile.RoomName ?? string.Empty,
            ["publishUrl"] = launchUrl ?? string.Empty
        };

        var isReady = !string.IsNullOrWhiteSpace(launchUrl);

        return new StreamingPublishDescriptor(
            ProviderId: Id,
            ProviderKind: Kind,
            DisplayName: DisplayName,
            IsReady: isReady,
            RequiresExternalRelay: false,
            Summary: isReady
                ? "Creates a push-ready VDO.Ninja session URL for browser publishing."
                : "Provide a VDO.Ninja publish URL or room name.",
            Parameters: parameters,
            LaunchUrl: launchUrl);
    }
}
