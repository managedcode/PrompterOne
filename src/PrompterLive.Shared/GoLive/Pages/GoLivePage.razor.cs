using Microsoft.AspNetCore.Components;
using PrompterLive.Core.Abstractions;
using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Services;
using PrompterLive.Shared.Services.Diagnostics;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage : ComponentBase
{
    private const string CustomRtmpReadySummary = "Use a custom relay, event CDN, or private ingest endpoint together with the rest of the stack.";
    private const string DefaultMicRouteLabel = "Monitor + Stream";
    private const string DisabledReadyPrefix = "Selected routing:";
    private const string DisabledStatusLabel = "Disabled";
    private const string DisabledSummary = "Enable this destination to arm it for the current program feed.";
    private const string GoLiveDefaultTitle = "Product Launch";
    private const string GoLiveLoadMessage = "Unable to prepare live routing right now.";
    private const string GoLiveLoadOperation = "Go Live load";
    private const string GoLiveSceneMessage = "Unable to save the current live scene.";
    private const string GoLiveSceneOperation = "Go Live save scene";
    private const string GoLiveStudioMessage = "Unable to save live routing settings.";
    private const string GoLiveStudioOperation = "Go Live save studio";
    private const string LocalRecordingReadySummary = "Capture the selected cameras locally while other live outputs stay armed.";
    private const string NeedsSetupStatusLabel = "Needs setup";
    private const string NdiReadySummary = "Expose the selected cameras over the network to switchers and remote studios.";
    private const string NoDestinationSourceSummary = "Select at least one scene camera for this destination.";
    private const string NoMicrophoneLabel = "No microphone";
    private const string ObsReadySummary = "Expose the selected cameras to OBS-compatible capture and virtual camera workflows.";
    private const string ReadyStatusLabel = "Ready";
    private const string SceneSettingsKey = "prompterlive.scene";
    private const string SelectedCameraSingularLabel = "selected camera";
    private const string SelectedCameraPluralLabel = "selected cameras";
    private const string StreamingSubtitle = "Program routing";

    [Inject] private AppBootstrapper Bootstrapper { get; set; } = null!;
    [Inject] private AppShellService Shell { get; set; } = null!;
    [Inject] private GoLiveSessionService GoLiveSession { get; set; } = null!;
    [Inject] private UiDiagnosticsService Diagnostics { get; set; } = null!;
    [Inject] private IMediaDeviceService MediaDeviceService { get; set; } = null!;
    [Inject] private IMediaSceneService MediaSceneService { get; set; } = null!;
    [Inject] private IScriptRepository ScriptRepository { get; set; } = null!;
    [Inject] private IScriptSessionService SessionService { get; set; } = null!;
    [Inject] private BrowserSettingsStore SettingsStore { get; set; } = null!;
    [Inject] private StudioSettingsStore StudioSettingsStore { get; set; } = null!;
    [Inject] private IEnumerable<IStreamingOutputProvider> StreamingProviders { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "id")]
    public string? ScriptId { get; set; }

    private bool _loadState = true;
    private string _screenSubtitle = StreamingSubtitle;
    private string _screenTitle = GoLiveDefaultTitle;
    private StudioSettings _studioSettings = StudioSettings.Default;

    private bool HasAnyLiveOutput =>
        _studioSettings.Streaming.ObsVirtualCameraEnabled
        || _studioSettings.Streaming.NdiOutputEnabled
        || _studioSettings.Streaming.LocalRecordingEnabled
        || _studioSettings.Streaming.LiveKitEnabled
        || _studioSettings.Streaming.VdoNinjaEnabled
        || _studioSettings.Streaming.YoutubeEnabled
        || _studioSettings.Streaming.TwitchEnabled
        || _studioSettings.Streaming.CustomRtmpEnabled;

    private bool HasPrimaryMicrophone => !string.IsNullOrWhiteSpace(MediaSceneService.State.PrimaryMicrophoneId);

    private bool HasScriptContext => !string.IsNullOrWhiteSpace(SessionService.State.ScriptId);

    private string LearnRoute => HasScriptContext
        ? AppRoutes.LearnWithId(SessionService.State.ScriptId)
        : AppRoutes.Learn;

    private GoLiveDestinationState CustomRtmpDescriptor => BuildRtmpDestinationState(
        _studioSettings.Streaming.CustomRtmpEnabled,
        _studioSettings.Streaming.CustomRtmpName,
        _studioSettings.Streaming.CustomRtmpUrl,
        _studioSettings.Streaming.CustomRtmpStreamKey,
        GoLiveTargetCatalog.TargetIds.CustomRtmp,
        CustomRtmpReadySummary);

    private string CustomRtmpStatusLabel => CustomRtmpDescriptor.StatusLabel;

    private string CustomRtmpSummary => CustomRtmpDescriptor.Summary;

    private GoLiveDestinationState LiveKitDescriptor => BuildLiveKitState();

    private string LiveKitStatusLabel => LiveKitDescriptor.StatusLabel;

    private string LiveKitSummary => LiveKitDescriptor.Summary;

    private GoLiveDestinationState NdiDescriptor => BuildLocalOutputState(
        _studioSettings.Streaming.NdiOutputEnabled,
        NdiReadySummary,
        GoLiveTargetCatalog.TargetIds.Ndi);

    private GoLiveDestinationState ObsDescriptor => BuildLocalOutputState(
        _studioSettings.Streaming.ObsVirtualCameraEnabled,
        ObsReadySummary,
        GoLiveTargetCatalog.TargetIds.Obs);

    private SceneCameraSource? PreviewCamera =>
        SceneCameras.FirstOrDefault(camera => camera.Transform.Visible && camera.Transform.IncludeInOutput)
        ?? SceneCameras.FirstOrDefault(camera => camera.Transform.Visible)
        ?? (SceneCameras.Count > 0 ? SceneCameras[0] : null);

    private string PrimaryMicrophoneLabel => MediaSceneService.State.PrimaryMicrophoneLabel ?? NoMicrophoneLabel;

    private string PrimaryMicrophoneRoute
    {
        get
        {
            var route = MediaSceneService.State.AudioBus.Inputs
                .FirstOrDefault(input => string.Equals(input.DeviceId, MediaSceneService.State.PrimaryMicrophoneId, StringComparison.Ordinal))
                ?.RouteTarget;

            return route is null
                ? DefaultMicRouteLabel
                : FormatRouteTarget(route.Value);
        }
    }

    private string ReadRoute => HasScriptContext
        ? AppRoutes.TeleprompterWithId(SessionService.State.ScriptId)
        : AppRoutes.Teleprompter;

    private GoLiveDestinationState RecordingDescriptor => BuildLocalOutputState(
        _studioSettings.Streaming.LocalRecordingEnabled,
        LocalRecordingReadySummary,
        GoLiveTargetCatalog.TargetIds.Recording);

    private IReadOnlyList<SceneCameraSource> SceneCameras => MediaSceneService.State.Cameras;

    private GoLiveDestinationState TwitchDescriptor => BuildRtmpDestinationState(
        _studioSettings.Streaming.TwitchEnabled,
        GoLiveTargetCatalog.TargetNames.Twitch,
        _studioSettings.Streaming.TwitchRtmpUrl,
        _studioSettings.Streaming.TwitchStreamKey,
        GoLiveTargetCatalog.TargetIds.Twitch,
        TwitchReadySummary);

    private string TwitchStatusLabel => TwitchDescriptor.StatusLabel;

    private string TwitchSummary => TwitchDescriptor.Summary;

    private GoLiveDestinationState VdoDescriptor => BuildVdoState();

    private string VdoStatusLabel => VdoDescriptor.StatusLabel;

    private string VdoSummary => VdoDescriptor.Summary;

    private GoLiveDestinationState YoutubeDescriptor => BuildRtmpDestinationState(
        _studioSettings.Streaming.YoutubeEnabled,
        GoLiveTargetCatalog.TargetNames.Youtube,
        _studioSettings.Streaming.YoutubeRtmpUrl,
        _studioSettings.Streaming.YoutubeStreamKey,
        GoLiveTargetCatalog.TargetIds.Youtube,
        YoutubeReadySummary);

    private string YoutubeStatusLabel => YoutubeDescriptor.StatusLabel;

    private string YoutubeSummary => YoutubeDescriptor.Summary;

    protected override Task OnParametersSetAsync()
    {
        _loadState = true;
        return Task.CompletedTask;
    }
}
