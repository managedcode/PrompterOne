using PrompterOne.Core.Abstractions;
using PrompterOne.Core.Models.Streaming;

namespace PrompterOne.Core.Services.Streaming;

public sealed class RtmpStreamingOutputProvider : IStreamingOutputProvider
{
    public string Id => "rtmp";

    public StreamingProviderKind Kind => StreamingProviderKind.Rtmp;

    public string DisplayName => "RTMP / RTMPS Relay";

    public StreamingPublishDescriptor Describe(StreamingProfile profile)
    {
        var destinations = profile.Destinations ?? Array.Empty<StreamingDestination>();
        var enabledDestinations = destinations.Where(destination => destination.IsEnabled).ToList();
        var isReady = enabledDestinations.Count > 0 && enabledDestinations.All(destination => !string.IsNullOrWhiteSpace(destination.Url));

        var parameters = enabledDestinations.ToDictionary(
            destination => destination.Name,
            destination => destination.Url,
            StringComparer.Ordinal);

        return new StreamingPublishDescriptor(
            ProviderId: Id,
            ProviderKind: Kind,
            DisplayName: DisplayName,
            IsReady: isReady,
            RequiresExternalRelay: true,
            Summary: isReady
                ? "Destinations are configured. Use a relay such as LiveKit egress or another RTMP bridge."
                : "Add at least one RTMP/RTMPS destination and route it through an external relay.",
            Parameters: parameters);
    }
}
