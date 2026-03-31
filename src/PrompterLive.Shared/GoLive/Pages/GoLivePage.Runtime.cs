using PrompterLive.Core.Models.Media;
using PrompterLive.Shared.Services;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage
{
    private const string GoLiveStartStreamMessage = "Unable to start live outputs right now.";
    private const string GoLiveStartStreamOperation = "Go Live start stream";
    private const string GoLiveStopStreamMessage = "Unable to stop live outputs right now.";
    private const string GoLiveStopStreamOperation = "Go Live stop stream";
    private const string GoLiveSwitchProgramMessage = "Unable to switch the live program source right now.";
    private const string GoLiveSwitchProgramOperation = "Go Live switch program source";

    private GoLiveOutputRuntimeRequest BuildRuntimeRequest(SceneCameraSource? camera) =>
        GoLiveOutputRequestFactory.Build(
            camera,
            MediaSceneService.State,
            _studioSettings.Streaming,
            _recordingPreferences,
            _screenTitle);
}
