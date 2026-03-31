using System.Globalization;
using Microsoft.AspNetCore.Components;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Core.Services.Streaming;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage
{
    private const string DestinationDisabledSummary = "Disabled until you arm this destination.";
    private const string DestinationDisabledStatusLabel = "Disabled";
    private const string DestinationEnabledStatusLabel = "Ready";
    private const string DestinationLocalSummarySuffix = " camera source(s) armed for this destination.";
    private const string DestinationNeedsSetupStatusLabel = "Needs setup";
    private const string DestinationNoSourceSummary = "Select at least one scene camera for this destination.";
    private const string DestinationRemoteReadySummary = "Credentials and source routing are stored in browser settings.";
    private const string DestinationRemoteSetupSummary = "Complete the destination fields and source routing before going live.";
    private const string GoLiveLiveKitDescription = "Keep LiveKit ingest credentials on the live screen so routing can be armed without leaving the studio.";
    private const string GoLiveProgramControlDescription = "Keep the active canvas output tuned before the live session starts.";
    private const string GoLiveProgramControlTitle = "Program Control";
    private const string GoLiveRecordingDescription = "Arm local recording before you start the program so the session can roll immediately.";
    private const string GoLiveYoutubeDescription = "Store YouTube RTMP credentials on the routing surface and arm sources next to the live canvas.";
    private const string ObsStudioDescription = "Keep the browser canvas available to OBS Virtual Camera for desktop capture workflows.";
    private const string OutputResolutionFieldLabel = "Output Resolution";
    private const string BitrateFieldLabel = "Bitrate (kbps)";
    private const string ShowTextOverlayFieldLabel = "Show Text Overlay";
    private const string IncludeCameraInOutputFieldLabel = "Include Camera in Output";
    private const string LiveKitServerFieldLabel = "Server URL";
    private const string LiveKitRoomFieldLabel = "Room Name";
    private const string LiveKitTokenFieldLabel = "Access Token";
    private const string YoutubeUrlFieldLabel = "RTMP / RTMPS URL";
    private const string YoutubeKeyFieldLabel = "Stream Key";
    private const string RoutingControlsDescription = "Arm destinations, tune the outbound program, and keep source routing beside the live canvas.";
    private const string RoutingControlsEyebrow = "Go Live Routing";
    private const string RoutingControlsTitle = "Program outputs";

    private string RoutingControlsSubtitle => HasAnyLiveOutput
        ? PrimarySessionBadge
        : DestinationDisabledStatusLabel;

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

    private async Task ToggleDestinationSourceAsync((string TargetId, string SourceId) update)
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = GoLiveDestinationRouting.ToggleSource(
                _studioSettings.Streaming,
                update.TargetId,
                update.SourceId,
                SceneCameras)
        };

        await PersistStudioSettingsAsync();
    }

    private async Task OnGoLiveOutputResolutionChanged(ChangeEventArgs args)
    {
        await EnsurePageReadyAsync();

        if (!Enum.TryParse<StreamingResolutionPreset>(args.Value?.ToString(), out var outputResolution))
        {
            return;
        }

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { OutputResolution = outputResolution }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateStreamingBitrateAsync(ChangeEventArgs args)
    {
        await EnsurePageReadyAsync();

        if (!int.TryParse(args.Value?.ToString(), out var bitrate))
        {
            return;
        }

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { BitrateKbps = Math.Max(250, bitrate) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleGoLiveTextOverlayAsync()
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                ShowTextOverlay = !_studioSettings.Streaming.ShowTextOverlay
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleGoLiveIncludeCameraAsync()
    {
        await EnsurePageReadyAsync();

        var nextValue = !_studioSettings.Streaming.IncludeCameraInOutput;
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { IncludeCameraInOutput = nextValue }
        };

        foreach (var camera in SceneCameras)
        {
            MediaSceneService.SetIncludeInOutput(camera.SourceId, nextValue);
        }

        await PersistSceneAsync();
        _studioSettings = _studioSettings with
        {
            Streaming = GoLiveDestinationRouting.Normalize(_studioSettings.Streaming, SceneCameras)
        };
        await PersistStudioSettingsAsync();
    }

    private async Task UpdateLiveKitServerAsync(ChangeEventArgs args)
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { LiveKitServerUrl = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateLiveKitRoomAsync(ChangeEventArgs args)
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { LiveKitRoomName = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateLiveKitTokenAsync(ChangeEventArgs args)
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { LiveKitToken = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateYoutubeUrlAsync(ChangeEventArgs args)
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { YoutubeRtmpUrl = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateYoutubeKeyAsync(ChangeEventArgs args)
    {
        await EnsurePageReadyAsync();

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { YoutubeStreamKey = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private static string GetInputValue(ChangeEventArgs args) => args.Value?.ToString() ?? string.Empty;
}
