namespace PrompterOne.Shared.Services;

public sealed record GoLiveOutputRuntimeState(
    bool LiveKitActive,
    bool ObsActive,
    bool RecordingActive,
    string CameraDeviceId,
    string MicrophoneDeviceId)
{
    public static GoLiveOutputRuntimeState Default { get; } = new(
        LiveKitActive: false,
        ObsActive: false,
        RecordingActive: false,
        CameraDeviceId: string.Empty,
        MicrophoneDeviceId: string.Empty);

    public bool HasActiveOutputs => HasLiveOutputs || RecordingActive;

    public bool HasLiveOutputs => LiveKitActive || ObsActive;
}
