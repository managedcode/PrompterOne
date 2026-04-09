using Microsoft.JSInterop;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Pages;

public partial class GoLivePage
{
    private async Task ReleaseCameraSurfacesAsync()
    {
        foreach (var videoDomId in EnumerateCameraSurfaceVideoIds())
        {
            await DetachCameraSurfaceAsync(videoDomId);
        }
    }

    private IEnumerable<string> EnumerateCameraSurfaceVideoIds()
    {
        var videoDomIds = new HashSet<string>(StringComparer.Ordinal)
        {
            UiDomIds.GoLive.ProgramVideo,
            UiDomIds.GoLive.PreviewVideo
        };

        foreach (var source in AvailableSceneSources)
        {
            if (!string.IsNullOrWhiteSpace(source.SourceId))
            {
                videoDomIds.Add(UiDomIds.GoLive.SourceVideo(source.SourceId));
            }
        }

        return videoDomIds;
    }

    private async Task DetachCameraSurfaceAsync(string videoDomId)
    {
        try
        {
            await CameraPreviewInterop.DetachCameraAsync(videoDomId);
        }
        catch (JSDisconnectedException)
        {
        }
    }
}
