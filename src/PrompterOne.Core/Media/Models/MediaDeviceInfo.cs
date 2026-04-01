namespace PrompterOne.Core.Models.Media;

public enum MediaDeviceKind
{
    Camera,
    Microphone,
    Speaker,
    Unknown
}

public sealed record MediaDeviceInfo(
    string DeviceId,
    string Label,
    MediaDeviceKind Kind,
    bool IsDefault = false);

public sealed record MediaPermissionsState(bool CameraGranted, bool MicrophoneGranted);
