namespace PrompterOne.Shared.Services;

internal sealed partial class GoLiveSessionService
{
    private readonly SemaphoreSlim _crossTabStartGate = new(1, 1);

    private bool _crossTabReady;
    private bool _disposed;
    private bool _stateRequestPublished;

    public async Task StartCrossTabSyncAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCrossTabReadyAsync(cancellationToken);

        if (_stateRequestPublished)
        {
            return;
        }

        _stateRequestPublished = true;
        await _crossTabMessageBus.PublishAsync(
            CrossTabMessageTypes.GoLiveSessionRequested,
            GoLiveSessionSyncRequest.Empty,
            cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _crossTabMessageBus.MessageReceived -= HandleCrossTabMessageAsync;
        _crossTabStartGate.Dispose();
    }

    private async Task EnsureCrossTabReadyAsync(CancellationToken cancellationToken)
    {
        if (_crossTabReady)
        {
            return;
        }

        await _crossTabStartGate.WaitAsync(cancellationToken);

        try
        {
            if (_crossTabReady)
            {
                return;
            }

            await _crossTabMessageBus.StartAsync(cancellationToken);
            _crossTabReady = true;
        }
        finally
        {
            _crossTabStartGate.Release();
        }
    }

    private async Task HandleCrossTabMessageAsync(CrossTabMessageEnvelope message)
    {
        if (string.Equals(message.MessageType, CrossTabMessageTypes.GoLiveSessionChanged, StringComparison.Ordinal))
        {
            var state = message.DeserializePayload<GoLiveSessionState>();
            if (state is not null)
            {
                SetState(state);
            }

            return;
        }

        if (!string.Equals(message.MessageType, CrossTabMessageTypes.GoLiveSessionRequested, StringComparison.Ordinal) ||
            !State.HasActiveSession)
        {
            return;
        }

        await PublishStateAsync(State);
    }

    private void PublishStateInBackground(GoLiveSessionState state)
    {
        _ = PublishStateAsync(state);
    }

    private async Task PublishStateAsync(GoLiveSessionState state, CancellationToken cancellationToken = default)
    {
        await EnsureCrossTabReadyAsync(cancellationToken);
        await _crossTabMessageBus.PublishAsync(
            CrossTabMessageTypes.GoLiveSessionChanged,
            state,
            cancellationToken);
    }
}
