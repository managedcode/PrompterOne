namespace PrompterLive.Shared.Pages;

public partial class GoLivePage : IDisposable
{
    private CancellationTokenSource? _sessionRefreshCancellation;
    private PeriodicTimer? _sessionRefreshTimer;

    protected override void OnInitialized()
    {
        GoLiveSession.StateChanged += HandleGoLiveSessionChanged;
        GoLiveOutputRuntime.StateChanged += HandleGoLiveOutputRuntimeChanged;
        UpdateSessionRefreshLoop();
    }

    public void Dispose()
    {
        GoLiveSession.StateChanged -= HandleGoLiveSessionChanged;
        GoLiveOutputRuntime.StateChanged -= HandleGoLiveOutputRuntimeChanged;
        StopSessionRefreshLoop();
        _interactionGate.Dispose();
    }

    private void HandleGoLiveSessionChanged()
    {
        UpdateSessionRefreshLoop();
        _ = InvokeAsync(StateHasChanged);
    }

    private void HandleGoLiveOutputRuntimeChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void UpdateSessionRefreshLoop()
    {
        if (GoLiveSession.State.HasActiveSession)
        {
            EnsureSessionRefreshLoop();
            return;
        }

        StopSessionRefreshLoop();
    }

    private void EnsureSessionRefreshLoop()
    {
        if (_sessionRefreshTimer is not null)
        {
            return;
        }

        _sessionRefreshCancellation = new CancellationTokenSource();
        _sessionRefreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _ = RunSessionRefreshLoopAsync(_sessionRefreshTimer, _sessionRefreshCancellation.Token);
    }

    private async Task RunSessionRefreshLoopAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void StopSessionRefreshLoop()
    {
        _sessionRefreshCancellation?.Cancel();
        _sessionRefreshCancellation?.Dispose();
        _sessionRefreshCancellation = null;
        _sessionRefreshTimer?.Dispose();
        _sessionRefreshTimer = null;
    }
}
