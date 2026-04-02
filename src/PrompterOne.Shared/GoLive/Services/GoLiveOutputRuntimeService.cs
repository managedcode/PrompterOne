namespace PrompterOne.Shared.Services;

public sealed class GoLiveOutputRuntimeService(GoLiveOutputInterop interop)
{
    private readonly GoLiveOutputInterop _interop = interop;

    public event Action? StateChanged;

    public GoLiveOutputRuntimeState State { get; private set; } = GoLiveOutputRuntimeState.Default;

    public async Task RefreshStateAsync()
    {
        var snapshot = await _interop.GetSessionStateAsync(GoLiveOutputRuntimeContract.SessionId);
        SetState(GoLiveOutputRuntimeState.FromSnapshot(snapshot));
    }

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
        await RefreshStateAsync();
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
        if (State.HasLiveOutputs)
        {
            await SyncLiveOutputsAsync(request);
            return;
        }

        await RefreshStateAsync();
    }

    public async Task StopStreamAsync()
    {
        if (State.LiveKitActive)
        {
            await _interop.StopLiveKitAsync(GoLiveOutputRuntimeContract.SessionId);
        }

        if (State.ObsActive)
        {
            await _interop.StopObsBrowserOutputAsync(GoLiveOutputRuntimeContract.SessionId);
        }

        await RefreshStateAsync();
    }

    public async Task StopRecordingAsync()
    {
        if (!State.RecordingActive)
        {
            return;
        }

        await _interop.StopLocalRecordingAsync(GoLiveOutputRuntimeContract.SessionId);
        await RefreshStateAsync();
    }

    private async Task SyncLiveOutputsAsync(GoLiveOutputRuntimeRequest request)
    {
        if (request.CanStartLiveKit)
        {
            await _interop.StartLiveKitAsync(
                GoLiveOutputRuntimeContract.SessionId,
                request);
        }
        else if (State.LiveKitActive)
        {
            await _interop.StopLiveKitAsync(GoLiveOutputRuntimeContract.SessionId);
        }

        if (request.CanStartObs)
        {
            await _interop.StartObsBrowserOutputAsync(
                GoLiveOutputRuntimeContract.SessionId,
                request);
        }
        else if (State.ObsActive)
        {
            await _interop.StopObsBrowserOutputAsync(GoLiveOutputRuntimeContract.SessionId);
        }

        await RefreshStateAsync();
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
