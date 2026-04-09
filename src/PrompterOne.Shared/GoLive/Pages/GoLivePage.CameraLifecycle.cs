namespace PrompterOne.Shared.Pages;

public partial class GoLivePage
{
    private async Task ReleaseCameraSurfacesAsync()
    {
        await CameraPreviewInterop.DetachAllCamerasAsync();
    }
}
