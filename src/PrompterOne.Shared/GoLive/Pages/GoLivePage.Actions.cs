using PrompterOne.Core.Models.Media;

namespace PrompterOne.Shared.Pages;

public partial class GoLivePage
{
    private async Task AddAvailableCameraAsync()
    {
        await EnsurePageReadyAsync();

        var nextCamera = _mediaDevices.FirstOrDefault(device =>
            device.Kind == MediaDeviceKind.Camera
            && SceneCameras.All(camera => !string.Equals(camera.DeviceId, device.DeviceId, StringComparison.Ordinal)));

        if (nextCamera is null)
        {
            return;
        }

        var source = MediaSceneService.AddCamera(nextCamera.DeviceId, nextCamera.Label);
        GoLiveSession.SelectSource(SceneCameras, source.SourceId);
        await PersistSceneAsync();
    }

    private async Task ToggleSceneOutputAsync(string sourceId)
    {
        await EnsurePageReadyAsync();

        var camera = SceneCameras.FirstOrDefault(item => string.Equals(item.SourceId, sourceId, StringComparison.Ordinal));
        if (camera is null)
        {
            return;
        }

        MediaSceneService.SetIncludeInOutput(sourceId, !camera.Transform.IncludeInOutput);
        await PersistSceneAsync();
    }
}
