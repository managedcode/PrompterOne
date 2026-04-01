using System.Globalization;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Core.Services.Streaming;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage
{
    private const string DestinationDisabledSummary = "Disabled in this live session.";
    private const string DestinationDisabledStatusLabel = "Disabled";
    private const string DestinationEnabledStatusLabel = "Ready";
    private const string DestinationLocalSummarySuffix = " source(s) armed for this output.";
    private const string DestinationNeedsSetupStatusLabel = "Needs setup";
    private const string DestinationNoSourceSummary = "No routed source is armed for this destination yet.";
    private const string DestinationRemoteReadySummary = "Credentials and source routing are ready in Settings.";
    private const string DestinationRemoteSetupSummary = "Complete provider setup in Settings before going live.";

    private IReadOnlyList<string> GetSelectedSourceIds(string targetId) =>
        GoLiveDestinationRouting.GetSelectedSourceIds(_studioSettings.Streaming, targetId, SceneCameras);

    private bool BuildDestinationIsReady(bool isEnabled, string targetId, params string[] requiredValues)
    {
        if (!isEnabled)
        {
            return false;
        }

        if (GetSelectedSourceIds(targetId).Count == 0)
        {
            return false;
        }

        return requiredValues.All(value => !string.IsNullOrWhiteSpace(value));
    }

    private string BuildLocalSummary(string targetId)
    {
        var selectedSources = GetSelectedSourceIds(targetId);
        return selectedSources.Count == 0
            ? DestinationNoSourceSummary
            : string.Concat(
                selectedSources.Count.ToString(CultureInfo.InvariantCulture),
                DestinationLocalSummarySuffix);
    }

    private string BuildRemoteSummary(bool isEnabled, string targetId, params string[] requiredValues)
    {
        if (!isEnabled)
        {
            return DestinationDisabledSummary;
        }

        if (GetSelectedSourceIds(targetId).Count == 0)
        {
            return DestinationNoSourceSummary;
        }

        return requiredValues.All(value => !string.IsNullOrWhiteSpace(value))
            ? DestinationRemoteReadySummary
            : DestinationRemoteSetupSummary;
    }

    private string BuildTargetStatusLabel(bool isEnabled, string targetId, params string[] requiredValues)
    {
        if (!isEnabled)
        {
            return DestinationDisabledStatusLabel;
        }

        return BuildDestinationIsReady(isEnabled, targetId, requiredValues)
            ? DestinationEnabledStatusLabel
            : DestinationNeedsSetupStatusLabel;
    }

    private async Task ToggleObsOutputAsync()
    {
        await RunSerializedInteractionAsync(async () =>
        {
            _studioSettings = _studioSettings with
            {
                Streaming = _studioSettings.Streaming with
                {
                    ObsVirtualCameraEnabled = !_studioSettings.Streaming.ObsVirtualCameraEnabled,
                    OutputMode = StreamingOutputMode.VirtualCamera
                }
            };

            await PersistStudioSettingsAsync();
        });
    }

    private async Task ToggleRecordingOutputAsync()
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                LocalRecordingEnabled = !_studioSettings.Streaming.LocalRecordingEnabled,
                OutputMode = StreamingOutputMode.LocalRecording
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleLiveKitSettingsAsync()
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                LiveKitEnabled = !_studioSettings.Streaming.LiveKitEnabled
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleYoutubeSettingsAsync()
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                YoutubeEnabled = !_studioSettings.Streaming.YoutubeEnabled,
                OutputMode = StreamingOutputMode.DirectRtmp
            }
        };

        await PersistStudioSettingsAsync();
    }
}
