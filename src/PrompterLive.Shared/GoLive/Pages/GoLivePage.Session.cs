using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Core.Services.Preview;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage
{
    private const string ActiveDestinationsIdleLabel = "No live destinations armed";
    private const string AudioTelemetryLabel = "Audio ready";
    private const string CameraFallbackLabel = "No camera selected";
    private const string DefaultProgramTimerLabel = "00:03:45";
    private const string ProgramBadgeIdleLabel = "Ready";
    private const string ProgramBadgeLiveLabel = "Live";
    private const string ProgramBadgeRecordingLabel = "Rec";
    private const string ProgramBadgeStreamingRecordingLabel = "Live + Rec";
    private const string RecordingButtonLabel = "Start Recording";
    private const string RecordingStopLabel = "Stop Recording";
    private const string SessionIdleLabel = "Ready";
    private const string SessionRecordingLabel = "Recording";
    private const string SessionStreamingLabel = "Streaming";
    private const string SessionStreamingRecordingLabel = "Streaming + Recording";
    private const string StageCaptionFallbackLead = "Good morning everyone,";
    private const string StageCaptionFallbackMiddle = "and welcome to what I believe";
    private const string StageCaptionFallbackEnd = "will be a transformative moment for our company.";
    private const string StageFrameRate30Label = "30 FPS";
    private const string StageFrameRate60Label = "60 FPS";
    private const string StreamButtonLabel = "Start Stream";
    private const string StreamStopLabel = "Stop Stream";
    private const string SwitchButtonDisabledLabel = "On Program";
    private const string SwitchButtonLabel = "Switch";

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

    private bool CanSwitchProgram => SelectedCamera is not null
        && !string.Equals(SelectedCamera.SourceId, ActiveCamera?.SourceId, StringComparison.Ordinal);

    private string PrimarySessionBadge => (GoLiveSession.State.IsStreamActive, GoLiveSession.State.IsRecordingActive) switch
    {
        (true, true) => ProgramBadgeStreamingRecordingLabel,
        (true, false) => ProgramBadgeLiveLabel,
        (false, true) => ProgramBadgeRecordingLabel,
        _ => ProgramBadgeIdleLabel
    };

    private string ProgramResolutionLabel => $"{ResolveResolutionDimensions(_studioSettings.Streaming.OutputResolution)} • {ActiveSourceLabel}";

    private static string ProgramTimerLabel => DefaultProgramTimerLabel;

    private string RecordingBadgeText => GoLiveSession.State.IsRecordingActive ? RecordingStopLabel : RecordingButtonLabel;

    private string SelectedSourceLabel => SelectedCamera?.Label ?? CameraFallbackLabel;

    private SceneCameraSource? SelectedCamera => ResolveSessionSource(GoLiveSession.State.SelectedSourceId) ?? ActiveCamera;

    private IReadOnlyList<string> StageCaptionLines => BuildStageCaptionLines();

    private string StageFrameRateLabel => BuildStageFrameRateLabel(_studioSettings.Streaming.OutputResolution);

    private string StreamActionLabel => GoLiveSession.State.IsStreamActive ? StreamStopLabel : StreamButtonLabel;

    private string SwitchActionLabel => CanSwitchProgram ? SwitchButtonLabel : SwitchButtonDisabledLabel;

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

    private static string ResolveResolutionDimensions(StreamingResolutionPreset resolution)
    {
        return resolution switch
        {
            StreamingResolutionPreset.FullHd1080p60 => "1920 × 1080",
            StreamingResolutionPreset.Hd720p30 => "1280 × 720",
            StreamingResolutionPreset.UltraHd2160p30 => "3840 × 2160",
            _ => "1920 × 1080"
        };
    }

    private static string BuildStageFrameRateLabel(StreamingResolutionPreset resolution) => resolution switch
    {
        StreamingResolutionPreset.FullHd1080p60 => StageFrameRate60Label,
        _ => StageFrameRate30Label
    };

    private IReadOnlyList<string> BuildStageCaptionLines()
    {
        if (SessionService.State.PreviewSegments.Count == 0)
        {
            return [StageCaptionFallbackLead, StageCaptionFallbackMiddle, StageCaptionFallbackEnd];
        }

        var segment = SessionService.State.PreviewSegments[0];
        var lines = ExtractCaptionLines(segment);
        return lines.Count == 0
            ? [StageCaptionFallbackLead, StageCaptionFallbackMiddle, StageCaptionFallbackEnd]
            : lines;
    }

    private static List<string> ExtractCaptionLines(SegmentPreviewModel segment)
    {
        var sourceText = string.IsNullOrWhiteSpace(segment.Content)
            ? segment.Blocks.FirstOrDefault()?.Text
            : segment.Content;
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            return [];
        }

        var normalized = sourceText
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return [];
        }

        var chunks = normalized
            .Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(chunk => !string.IsNullOrWhiteSpace(chunk))
            .Take(3)
            .ToList();

        if (chunks.Count == 0)
        {
            return [];
        }

        for (var index = 0; index < chunks.Count; index++)
        {
            chunks[index] = chunks[index].Trim();
        }

        return chunks;
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
