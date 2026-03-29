using PrompterLive.Core.Models.Streaming;
using PrompterLive.Core.Services.Streaming;

namespace PrompterLive.Core.Tests;

public sealed class StreamingProviderTests
{
    [Fact]
    public void LiveKitProvider_ReportsReadyOnlyWhenAllCredentialsExist()
    {
        var provider = new LiveKitOutputProvider();

        var pendingDescriptor = provider.Describe(StreamingProfile.CreateDefault(StreamingProviderKind.LiveKit));
        var readyDescriptor = provider.Describe(StreamingProfile.CreateDefault(StreamingProviderKind.LiveKit) with
        {
            ServerUrl = "wss://livekit.example.com",
            RoomName = "prompter-room",
            Token = "token-value"
        });

        Assert.False(pendingDescriptor.IsReady);
        Assert.True(readyDescriptor.IsReady);
        Assert.Equal("configured", readyDescriptor.Parameters["token"]);
    }

    [Fact]
    public void VdoNinjaProvider_BuildsLaunchUrlFromRoomName()
    {
        var provider = new VdoNinjaOutputProvider();
        var profile = StreamingProfile.CreateDefault(StreamingProviderKind.VdoNinja) with
        {
            RoomName = "prompter-live"
        };

        var descriptor = provider.Describe(profile);

        Assert.True(descriptor.IsReady);
        Assert.NotNull(descriptor.LaunchUrl);
        Assert.Contains("room=prompter-live", descriptor.LaunchUrl, StringComparison.Ordinal);
        Assert.Contains("push=prompter-live", descriptor.LaunchUrl, StringComparison.Ordinal);
    }

    [Fact]
    public void RtmpProvider_RequiresExternalRelayAndEnabledDestination()
    {
        var provider = new RtmpStreamingOutputProvider();
        var descriptor = provider.Describe(StreamingProfile.CreateDefault(StreamingProviderKind.Rtmp) with
        {
            Destinations =
            [
                new StreamingDestination("YouTube", "rtmps://a.rtmp.youtube.com/live2", "stream-key", true)
            ]
        });

        Assert.True(descriptor.IsReady);
        Assert.True(descriptor.RequiresExternalRelay);
        Assert.Equal("rtmps://a.rtmp.youtube.com/live2", descriptor.Parameters["YouTube"]);
    }
}
