using Microsoft.AspNetCore.Components;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Core.Services.Streaming;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage
{
    private const string DirectRtmpOutputModeValue = "direct-rtmp";
    private const string LocalRecordingOutputModeValue = "local-recording";
    private const string NdiOutputModeValue = "ndi-output";
    private const string VirtualCameraOutputModeValue = "virtual-camera";

    private string SelectedOutputModeValue => _studioSettings.Streaming.OutputMode switch
    {
        StreamingOutputMode.DirectRtmp => DirectRtmpOutputModeValue,
        StreamingOutputMode.LocalRecording => LocalRecordingOutputModeValue,
        StreamingOutputMode.NdiOutput => NdiOutputModeValue,
        _ => VirtualCameraOutputModeValue
    };

    private async Task ToggleLiveKitAsync()
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                LiveKitEnabled = !_studioSettings.Streaming.LiveKitEnabled
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleVdoNinjaAsync()
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                VdoNinjaEnabled = !_studioSettings.Streaming.VdoNinjaEnabled
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleYoutubeAsync()
    {
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

    private async Task ToggleTwitchAsync()
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                TwitchEnabled = !_studioSettings.Streaming.TwitchEnabled,
                OutputMode = StreamingOutputMode.DirectRtmp
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleCustomRtmpAsync()
    {
        var customName = string.IsNullOrWhiteSpace(_studioSettings.Streaming.CustomRtmpName)
            ? StreamingDefaults.CustomTargetName
            : _studioSettings.Streaming.CustomRtmpName;

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                CustomRtmpEnabled = !_studioSettings.Streaming.CustomRtmpEnabled,
                CustomRtmpName = customName,
                OutputMode = StreamingOutputMode.DirectRtmp,
                RtmpUrl = _studioSettings.Streaming.CustomRtmpUrl,
                StreamKey = _studioSettings.Streaming.CustomRtmpStreamKey
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleSceneOutputAsync(string sourceId)
    {
        var camera = SceneCameras.FirstOrDefault(item => string.Equals(item.SourceId, sourceId, StringComparison.Ordinal));
        if (camera is null)
        {
            return;
        }

        MediaSceneService.SetIncludeInOutput(sourceId, !camera.Transform.IncludeInOutput);
        await PersistSceneAsync();
    }

    private async Task ToggleDestinationSourceAsync(string targetId, string sourceId)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = GoLiveDestinationRouting.ToggleSource(
                _studioSettings.Streaming,
                targetId,
                sourceId,
                SceneCameras)
        };

        await PersistStudioSettingsAsync();
    }

    private async Task OnOutputResolutionChanged(ChangeEventArgs args)
    {
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

    private async Task OnOutputModeChanged(ChangeEventArgs args)
    {
        var nextMode = args.Value?.ToString() switch
        {
            DirectRtmpOutputModeValue => StreamingOutputMode.DirectRtmp,
            LocalRecordingOutputModeValue => StreamingOutputMode.LocalRecording,
            NdiOutputModeValue => StreamingOutputMode.NdiOutput,
            _ => StreamingOutputMode.VirtualCamera
        };

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { OutputMode = nextMode }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateStreamingBitrateAsync(ChangeEventArgs args)
    {
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

    private async Task ToggleStreamTextOverlayAsync()
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                ShowTextOverlay = !_studioSettings.Streaming.ShowTextOverlay
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task ToggleStreamIncludeCameraAsync()
    {
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
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { LiveKitServerUrl = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateLiveKitRoomAsync(ChangeEventArgs args)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { LiveKitRoomName = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateLiveKitTokenAsync(ChangeEventArgs args)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { LiveKitToken = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateVdoNinjaRoomAsync(ChangeEventArgs args)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { VdoNinjaRoomName = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateVdoNinjaPublishUrlAsync(ChangeEventArgs args)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { VdoNinjaPublishUrl = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateYoutubeUrlAsync(ChangeEventArgs args)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { YoutubeRtmpUrl = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateYoutubeKeyAsync(ChangeEventArgs args)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { YoutubeStreamKey = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateTwitchUrlAsync(ChangeEventArgs args)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { TwitchRtmpUrl = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateTwitchKeyAsync(ChangeEventArgs args)
    {
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { TwitchStreamKey = GetInputValue(args) }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateCustomRtmpNameAsync(ChangeEventArgs args)
    {
        var nextName = string.IsNullOrWhiteSpace(GetInputValue(args))
            ? StreamingDefaults.CustomTargetName
            : GetInputValue(args);

        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with { CustomRtmpName = nextName }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateCustomRtmpUrlAsync(ChangeEventArgs args)
    {
        var nextValue = GetInputValue(args);
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                CustomRtmpUrl = nextValue,
                RtmpUrl = nextValue
            }
        };

        await PersistStudioSettingsAsync();
    }

    private async Task UpdateCustomRtmpKeyAsync(ChangeEventArgs args)
    {
        var nextValue = GetInputValue(args);
        _studioSettings = _studioSettings with
        {
            Streaming = _studioSettings.Streaming with
            {
                CustomRtmpStreamKey = nextValue,
                StreamKey = nextValue
            }
        };

        await PersistStudioSettingsAsync();
    }
}
