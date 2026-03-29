namespace PrompterLive.Core.Models.Workspace;

public sealed record ReaderSettings(
    int CountdownSeconds = 3,
    double FontScale = 1.0,
    double TextWidth = 0.72,
    double ScrollSpeed = 1.0,
    bool MirrorText = false,
    bool ShowFocusLine = true,
    bool ShowProgress = true,
    bool ShowCameraScene = true);
