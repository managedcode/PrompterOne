namespace PrompterLive.Shared.Services;

public sealed class GoLiveOutputRuntimeService(GoLiveOutputInterop interop)
{
    private readonly GoLiveOutputInterop _interop = interop;

    public event Action? StateChanged;

    public GoLiveOutputRuntimeState State { get; private set; } = GoLiveOutputRuntimeState.Default;

    public async Task StartStreamAsync(GoLiveOutputRuntimeRequest request)
    {
        await SyncLiveOutputsAsync(request);
    }

    public async Task StartRecordingAsync(GoLiveOutputRuntimeRequest request)
    {
        if (!request.CanStartRecording)
        {
            return;
        }

        await _interop.StartLocalRecordingAsync(
            GoLiveOutputRuntimeContract.SessionId,
            request);

        SetState(State with
        {
            RecordingActive = true,
            CameraDeviceId = request.PrimaryCameraDeviceId,
            MicrophoneDeviceId = request.PrimaryMicrophoneDeviceId
        });
    }

    public async Task UpdateProgramSourceAsync(GoLiveOutputRuntimeRequest request)
    {
        if (!State.HasActiveOutputs)
        {
            return;
        }

        await _interop.UpdateSessionDevicesAsync(
            GoLiveOutputRuntimeContract.SessionId,
            request);

        if (!State.HasLiveOutputs)
        {
            SetState(State with
            {
                CameraDeviceId = request.PrimaryCameraDeviceId,
                MicrophoneDeviceId = request.PrimaryMicrophoneDeviceId
            });
            return;
        }

        await SyncLiveOutputsAsync(request);
    }

    public async Task StopStreamAsync()
    {
        var nextState = State;

        if (State.LiveKitActive)
        {
            await _interop.StopLiveKitAsync(GoLiveOutputRuntimeContract.SessionId);
            nextState = nextState with { LiveKitActive = false };
        }

        if (State.ObsActive)
        {
            await _interop.StopObsBrowserOutputAsync(GoLiveOutputRuntimeContract.SessionId);
            nextState = nextState with { ObsActive = false };
        }

        SetState(nextState.HasActiveOutputs
            ? nextState
            : GoLiveOutputRuntimeState.Default);
    }

    public async Task StopRecordingAsync()
    {
        if (!State.RecordingActive)
        {
            return;
        }

        await _interop.StopLocalRecordingAsync(GoLiveOutputRuntimeContract.SessionId);
        var nextState = State with { RecordingActive = false };
        SetState(nextState.HasActiveOutputs
            ? nextState
            : GoLiveOutputRuntimeState.Default);
    }

    private async Task SyncLiveOutputsAsync(GoLiveOutputRuntimeRequest request)
    {
        var nextState = State with
        {
            CameraDeviceId = request.PrimaryCameraDeviceId,
            MicrophoneDeviceId = request.PrimaryMicrophoneDeviceId
        };

        if (request.CanStartLiveKit)
        {
            await _interop.StartLiveKitAsync(
                GoLiveOutputRuntimeContract.SessionId,
                request);
            nextState = nextState with { LiveKitActive = true };
        }
        else if (State.LiveKitActive)
        {
            await _interop.StopLiveKitAsync(GoLiveOutputRuntimeContract.SessionId);
            nextState = nextState with { LiveKitActive = false };
        }

        if (request.CanStartObs)
        {
            await _interop.StartObsBrowserOutputAsync(
                GoLiveOutputRuntimeContract.SessionId,
                request);
            nextState = nextState with { ObsActive = true };
        }
        else if (State.ObsActive)
        {
            await _interop.StopObsBrowserOutputAsync(GoLiveOutputRuntimeContract.SessionId);
            nextState = nextState with { ObsActive = false };
        }

        SetState(nextState);
    }

    private void SetState(GoLiveOutputRuntimeState nextState)
    {
        if (EqualityComparer<GoLiveOutputRuntimeState>.Default.Equals(State, nextState))
        {
            return;
        }

        State = nextState;
        StateChanged?.Invoke();
    }
}
