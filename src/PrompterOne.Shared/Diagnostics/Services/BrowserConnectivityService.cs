using Microsoft.Extensions.Localization;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Services.Diagnostics;

public sealed class BrowserConnectivityService(
    BrowserConnectivityInterop connectivityInterop,
    IStringLocalizer<SharedResource> localizer) : IDisposable, IAsyncDisposable
{
    private const int OnlineAutoHideDelayMilliseconds = 2400;
    private const int PollIntervalMilliseconds = 1000;

    private readonly BrowserConnectivityInterop _connectivityInterop = connectivityInterop;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;
    private readonly SemaphoreSlim _startGate = new(1, 1);

    private CancellationTokenSource? _hideCts;
    private CancellationTokenSource? _monitorCts;
    private bool? _lastKnownOnline;
    private Task? _monitorTask;
    private bool _disposed;

    public event EventHandler? Changed;

    public string Message { get; private set; } = string.Empty;

    public string State { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public bool IsVisible { get; private set; }

    public async Task StartAsync()
    {
        if (_monitorTask is not null)
        {
            return;
        }

        await _startGate.WaitAsync();

        try
        {
            if (_monitorTask is not null)
            {
                return;
            }

            _monitorCts = new CancellationTokenSource();
            _monitorTask = MonitorAsync(_monitorCts.Token);
        }
        finally
        {
            _startGate.Release();
        }
    }

    public void Dismiss()
    {
        CancelPendingHide();
        UpdateState(isVisible: false, state: string.Empty, title: string.Empty, message: string.Empty);
    }

    public void Dispose()
    {
        DisposeMonitorResources();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        var monitorTask = _monitorTask;
        DisposeMonitorResources();

        if (monitorTask is not null)
        {
            try
            {
                await monitorTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        GC.SuppressFinalize(this);
    }

    private async Task MonitorAsync(CancellationToken cancellationToken)
    {
        await ProbeAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(PollIntervalMilliseconds, cancellationToken);
                await ProbeAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProbeAsync(CancellationToken cancellationToken)
    {
        var previousOnlineState = _lastKnownOnline;
        var isOnline = await _connectivityInterop.GetOnlineStatusAsync(cancellationToken);
        if (!isOnline.HasValue)
        {
            return;
        }

        if (previousOnlineState == isOnline.Value)
        {
            return;
        }

        _lastKnownOnline = isOnline.Value;

        if (!isOnline.Value)
        {
            CancelPendingHide();
            UpdateState(
                isVisible: true,
                state: ConnectivityStateValues.Offline,
                title: Text(UiTextKey.DiagnosticsConnectivityOfflineTitle),
                message: Text(UiTextKey.DiagnosticsConnectivityOfflineMessage));

            return;
        }

        if (previousOnlineState.HasValue)
        {
            UpdateState(
                isVisible: true,
                state: ConnectivityStateValues.Online,
                title: Text(UiTextKey.DiagnosticsConnectivityOnlineTitle),
                message: Text(UiTextKey.DiagnosticsConnectivityOnlineMessage));

            ScheduleHide();
        }
    }

    private void ScheduleHide()
    {
        CancelPendingHide();
        _hideCts = new CancellationTokenSource();
        _ = HideAsync(_hideCts.Token);
    }

    private async Task HideAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(OnlineAutoHideDelayMilliseconds, cancellationToken);
            UpdateState(isVisible: false, state: string.Empty, title: string.Empty, message: string.Empty);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CancelPendingHide()
    {
        _hideCts?.Cancel();
        _hideCts?.Dispose();
        _hideCts = null;
    }

    private void DisposeMonitorResources()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CancelPendingHide();

        if (_monitorCts is not null)
        {
            _monitorCts.Cancel();
            _monitorCts.Dispose();
            _monitorCts = null;
        }

        _startGate.Dispose();
    }

    private void UpdateState(bool isVisible, string state, string title, string message)
    {
        if (IsVisible == isVisible &&
            string.Equals(State, state, StringComparison.Ordinal) &&
            string.Equals(Title, title, StringComparison.Ordinal) &&
            string.Equals(Message, message, StringComparison.Ordinal))
        {
            return;
        }

        IsVisible = isVisible;
        State = state;
        Title = title;
        Message = message;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private string Text(UiTextKey key) => _localizer[key.ToString()];

    private static class ConnectivityStateValues
    {
        public const string Offline = "offline";
        public const string Online = "online";
    }
}
