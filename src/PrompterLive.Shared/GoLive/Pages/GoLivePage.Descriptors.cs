using Microsoft.AspNetCore.Components;
using PrompterLive.Core.Abstractions;
using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Streaming;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Core.Services.Streaming;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage
{
    private const string LiveKitReadySummary = "Publish the selected cameras to a LiveKit room.";
    private const string TwitchReadySummary = "Keep the selected cameras armed for Twitch in the same live session.";
    private const string VdoNinjaReadySummary = "Create a browser publishing session for the selected cameras in parallel with teleprompter.";
    private const string YoutubeReadySummary = "Arm the selected cameras for a YouTube RTMP relay.";

    private GoLiveDestinationState BuildLiveKitState()
    {
        var provider = GetProvider(StreamingProviderKind.LiveKit);
        var descriptor = provider.Describe(
            StreamingProfile.CreateDefault(StreamingProviderKind.LiveKit) with
            {
                ServerUrl = _studioSettings.Streaming.LiveKitServerUrl,
                RoomName = _studioSettings.Streaming.LiveKitRoomName,
                Token = _studioSettings.Streaming.LiveKitToken
            });

        return BuildProviderDestinationState(
            _studioSettings.Streaming.LiveKitEnabled,
            descriptor,
            GoLiveTargetCatalog.TargetIds.LiveKit,
            LiveKitReadySummary);
    }

    private GoLiveDestinationState BuildVdoState()
    {
        var provider = GetProvider(StreamingProviderKind.VdoNinja);
        var descriptor = provider.Describe(
            StreamingProfile.CreateDefault(StreamingProviderKind.VdoNinja) with
            {
                RoomName = _studioSettings.Streaming.VdoNinjaRoomName,
                PublishUrl = _studioSettings.Streaming.VdoNinjaPublishUrl
            });

        return BuildProviderDestinationState(
            _studioSettings.Streaming.VdoNinjaEnabled,
            descriptor,
            GoLiveTargetCatalog.TargetIds.VdoNinja,
            VdoNinjaReadySummary);
    }

    private GoLiveDestinationState BuildRtmpDestinationState(
        bool isEnabled,
        string targetName,
        string targetUrl,
        string targetKey,
        string targetId,
        string readySummary)
    {
        var provider = GetProvider(StreamingProviderKind.Rtmp);
        var descriptor = provider.Describe(
            StreamingProfile.CreateDefault(StreamingProviderKind.Rtmp) with
            {
                Name = targetName,
                Destinations =
                [
                    new StreamingDestination(targetName, targetUrl, targetKey, isEnabled)
                ]
            });

        return BuildProviderDestinationState(isEnabled, descriptor, targetId, readySummary);
    }

    private GoLiveDestinationState BuildProviderDestinationState(
        bool isEnabled,
        StreamingPublishDescriptor descriptor,
        string targetId,
        string readySummary)
    {
        var selectedSourceIds = GetDestinationSourceIds(targetId);
        if (!isEnabled)
        {
            return new GoLiveDestinationState(
                false,
                selectedSourceIds.Count > 0 && descriptor.IsReady,
                DisabledStatusLabel,
                BuildDisabledSummary(selectedSourceIds.Count));
        }

        if (selectedSourceIds.Count == 0)
        {
            return new GoLiveDestinationState(true, false, NeedsSetupStatusLabel, NoDestinationSourceSummary);
        }

        return descriptor.IsReady
            ? new GoLiveDestinationState(true, true, ReadyStatusLabel, BuildReadySummary(selectedSourceIds.Count, readySummary))
            : new GoLiveDestinationState(true, false, NeedsSetupStatusLabel, descriptor.Summary);
    }

    private GoLiveDestinationState BuildLocalOutputState(
        bool isEnabled,
        string readySummary,
        string targetId)
    {
        var selectedSourceIds = GetDestinationSourceIds(targetId);
        if (!isEnabled)
        {
            return new GoLiveDestinationState(
                false,
                selectedSourceIds.Count > 0,
                DisabledStatusLabel,
                BuildDisabledSummary(selectedSourceIds.Count));
        }

        return selectedSourceIds.Count > 0
            ? new GoLiveDestinationState(true, true, ReadyStatusLabel, BuildReadySummary(selectedSourceIds.Count, readySummary))
            : new GoLiveDestinationState(true, false, NeedsSetupStatusLabel, NoDestinationSourceSummary);
    }

    private IReadOnlyList<string> GetDestinationSourceIds(string targetId) =>
        GoLiveDestinationRouting.GetSelectedSourceIds(_studioSettings.Streaming, targetId, SceneCameras);

    private IStreamingOutputProvider GetProvider(StreamingProviderKind kind) =>
        StreamingProviders.First(provider => provider.Kind == kind);

    private static string GetInputValue(ChangeEventArgs args) => args.Value?.ToString() ?? string.Empty;

    private static string FormatRouteTarget(AudioRouteTarget routeTarget) =>
        routeTarget switch
        {
            AudioRouteTarget.Monitor => "Monitor only",
            AudioRouteTarget.Stream => "Stream only",
            _ => DefaultMicRouteLabel
        };

    private static string BuildSelectedSourceSummary(int selectedSourceCount)
    {
        return string.Concat(
            selectedSourceCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            " ",
            selectedSourceCount == 1 ? SelectedCameraSingularLabel : SelectedCameraPluralLabel);
    }

    private static string BuildDisabledSummary(int selectedSourceCount)
    {
        return selectedSourceCount == 0
            ? DisabledSummary
            : string.Concat(DisabledReadyPrefix, " ", BuildSelectedSourceSummary(selectedSourceCount), ".");
    }

    private static string BuildReadySummary(int selectedSourceCount, string readySummary) =>
        string.Concat(BuildSelectedSourceSummary(selectedSourceCount), ". ", readySummary);

    private sealed record GoLiveDestinationState(
        bool IsEnabled,
        bool IsReady,
        string StatusLabel,
        string Summary);
}
