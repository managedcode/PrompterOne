using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Workspace;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage
{
    private const string ActiveDestinationsIdleLabel = "No live destinations armed";
    private const string AudioTelemetryLabel = "Audio ready";
    private const string CameraFallbackLabel = "No camera selected";
    private const string RecordingButtonLabel = "Start Recording";
    private const string RecordingStopLabel = "Stop Recording";
    private const string SessionIdleLabel = "Ready";
    private const string SessionRecordingLabel = "Recording";
    private const string SessionStreamingLabel = "Streaming";
    private const string SessionStreamingRecordingLabel = "Streaming + Recording";
    private const string StreamButtonLabel = "Start Stream";
    private const string StreamStopLabel = "Stop Stream";
    private const string SwitchButtonDisabledLabel = "On Program";
    private const string SwitchButtonLabel = "Switch Selected";

    private SceneCameraSource? ActiveCamera => ResolveSessionSource(GoLiveSession.State.ActiveSourceId) ?? PreviewCamera;

    private string ActiveDestinationsTelemetry => BuildActiveDestinationsTelemetry();

    private string ActiveSessionLabel => (GoLiveSession.State.IsStreamActive, GoLiveSession.State.IsRecordingActive) switch
    {
        (true, true) => SessionStreamingRecordingLabel,
        (true, false) => SessionStreamingLabel,
        (false, true) => SessionRecordingLabel,
        _ => SessionIdleLabel
    };

    private string ActiveSourceLabel => ActiveCamera?.Label ?? CameraFallbackLabel;

    private bool CanControlProgram => SelectedCamera is not null;

    private string SelectedSourceLabel => SelectedCamera?.Label ?? CameraFallbackLabel;

    private SceneCameraSource? SelectedCamera => ResolveSessionSource(GoLiveSession.State.SelectedSourceId) ?? ActiveCamera;

    private string StreamActionLabel => GoLiveSession.State.IsStreamActive ? StreamStopLabel : StreamButtonLabel;

    private string RecordingActionLabel => GoLiveSession.State.IsRecordingActive ? RecordingStopLabel : RecordingButtonLabel;

    private string SwitchActionLabel => SelectedCamera is null || string.Equals(SelectedCamera.SourceId, ActiveCamera?.SourceId, StringComparison.Ordinal)
        ? SwitchButtonDisabledLabel
        : SwitchButtonLabel;

    private string VideoTelemetry => $"{FormatOutputResolution(_studioSettings.Streaming.OutputResolution)} · {ActiveSourceLabel}";

    private string AudioTelemetry => HasPrimaryMicrophone
        ? $"{PrimaryMicrophoneLabel} · {PrimaryMicrophoneRoute}"
        : AudioTelemetryLabel;

    private string BitrateTelemetry => $"{_studioSettings.Streaming.BitrateKbps} kbps";

    private void SyncGoLiveSessionState()
    {
        GoLiveSession.EnsureSession(
            SessionService.State.ScriptId,
            _screenTitle,
            _screenSubtitle,
            PrimaryMicrophoneLabel,
            _studioSettings.Streaming,
            SceneCameras);
    }

    private Task SelectSourceAsync(string sourceId)
    {
        GoLiveSession.SelectSource(SceneCameras, sourceId);
        return Task.CompletedTask;
    }

    private Task SwitchSelectedSourceAsync()
    {
        GoLiveSession.SwitchToSelectedSource(SceneCameras);
        return Task.CompletedTask;
    }

    private Task ToggleStreamSessionAsync()
    {
        GoLiveSession.ToggleStream(SceneCameras);
        return Task.CompletedTask;
    }

    private Task ToggleRecordingSessionAsync()
    {
        GoLiveSession.ToggleRecording(SceneCameras);
        return Task.CompletedTask;
    }

    private SceneCameraSource? ResolveSessionSource(string sourceId)
    {
        return SceneCameras.FirstOrDefault(camera => string.Equals(camera.SourceId, sourceId, StringComparison.Ordinal));
    }

    private static string FormatOutputResolution(StreamingResolutionPreset resolution)
    {
        return resolution switch
        {
            StreamingResolutionPreset.FullHd1080p60 => "1080p60",
            StreamingResolutionPreset.Hd720p30 => "720p30",
            StreamingResolutionPreset.UltraHd2160p30 => "2160p30",
            _ => "1080p30"
        };
    }

    private string BuildActiveDestinationsTelemetry()
    {
        var names = new List<string>();

        if (_studioSettings.Streaming.ObsVirtualCameraEnabled)
        {
            names.Add("OBS");
        }

        if (_studioSettings.Streaming.NdiOutputEnabled)
        {
            names.Add("NDI");
        }

        if (_studioSettings.Streaming.LiveKitEnabled)
        {
            names.Add("LiveKit");
        }

        if (_studioSettings.Streaming.VdoNinjaEnabled)
        {
            names.Add("VDO");
        }

        if (_studioSettings.Streaming.YoutubeEnabled)
        {
            names.Add("YouTube");
        }

        if (_studioSettings.Streaming.TwitchEnabled)
        {
            names.Add("Twitch");
        }

        if (_studioSettings.Streaming.CustomRtmpEnabled)
        {
            names.Add(_studioSettings.Streaming.CustomRtmpName);
        }

        return names.Count == 0
            ? ActiveDestinationsIdleLabel
            : string.Join(" · ", names);
    }
}
