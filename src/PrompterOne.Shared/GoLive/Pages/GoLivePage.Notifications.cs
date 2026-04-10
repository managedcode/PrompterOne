using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.GoLive.Models;

namespace PrompterOne.Shared.Pages;

public partial class GoLivePage
{
    private const string BackgroundMaintenanceOperation = "GoLiveBackgroundMaintenance";
    private static readonly string GoLiveRoutePrefix = AppRoutes.GoLive.TrimStart('/');
    private static readonly TimeSpan SessionRefreshInterval = TimeSpan.FromMilliseconds(200);

    private IDisposable? _locationChangingRegistration;
    private CancellationTokenSource? _sessionRefreshCancellation;
    private PeriodicTimer? _sessionRefreshTimer;
    private bool _disposed;

    protected override void OnInitialized()
    {
        GoLiveSession.StateChanged += HandleGoLiveSessionChanged;
        GoLiveOutputRuntime.StateChanged += HandleGoLiveOutputRuntimeChanged;
        GoLiveRemoteSourceRuntime.StateChanged += HandleGoLiveRemoteSourceRuntimeChanged;
        _locationChangingRegistration = Navigation.RegisterLocationChangingHandler(HandleLocationChangingAsync);
        UpdateSessionRefreshLoop();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        RunCleanupInBackground(ReleaseCameraSurfacesAsync);
        RunCleanupInBackground(StopPrimaryMicrophoneMonitorAsync);
        RunCleanupInBackground(GoLiveRemoteSourceRuntime.StopAsync);
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await RunCleanupSafelyAsync(ReleaseCameraSurfacesAsync);
        await RunCleanupSafelyAsync(StopPrimaryMicrophoneMonitorAsync);
        await RunCleanupSafelyAsync(GoLiveRemoteSourceRuntime.StopAsync);
        DisposeCore();
        GC.SuppressFinalize(this);
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

    private void HandleGoLiveRemoteSourceRuntimeChanged()
    {
        ApplyRemoteSourceState();
        _ = InvokeAsync(StateHasChanged);
    }

    private void UpdateSessionRefreshLoop()
    {
        if (GoLiveSession.State.HasActiveSession || HasSourceIntakeConnections)
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
        _sessionRefreshTimer = new PeriodicTimer(SessionRefreshInterval);
        _ = RunSessionRefreshLoopAsync(_sessionRefreshTimer, _sessionRefreshCancellation.Token);
    }

    private async Task RunSessionRefreshLoopAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                if (HasSourceIntakeConnections)
                {
                    await InvokeAsync(SyncRemoteSourcesAsync);
                }

                if (GoLiveSession.State.HasActiveSession)
                {
                    await InvokeAsync(GoLiveOutputRuntime.RefreshStateAsync);
                }

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception) when (IsIgnorableBackgroundFailure(exception))
        {
        }
        catch (Exception exception)
        {
            ReportBackgroundFailure(exception);
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

    private ValueTask HandleLocationChangingAsync(LocationChangingContext context)
    {
        if (_disposed || IsGoLiveTarget(context.TargetLocation))
        {
            return ValueTask.CompletedTask;
        }

        StopSessionRefreshLoop();
        RunCleanupInBackground(ReleaseCameraSurfacesAsync);
        RunCleanupInBackground(StopPrimaryMicrophoneMonitorAsync);
        return ValueTask.CompletedTask;
    }

    private bool IsGoLiveTarget(string targetLocation)
    {
        var relativePath = Navigation.ToBaseRelativePath(targetLocation);
        return relativePath.StartsWith(GoLiveRoutePrefix, StringComparison.OrdinalIgnoreCase);
    }

    private void DisposeCore()
    {
        GoLiveSession.StateChanged -= HandleGoLiveSessionChanged;
        GoLiveOutputRuntime.StateChanged -= HandleGoLiveOutputRuntimeChanged;
        GoLiveRemoteSourceRuntime.StateChanged -= HandleGoLiveRemoteSourceRuntimeChanged;
        _locationChangingRegistration?.Dispose();
        _locationChangingRegistration = null;
        StopSessionRefreshLoop();
        DisposePrimaryMicrophoneObserver();
        _interactionGate.Dispose();
    }

    private void RunCleanupInBackground(Func<Task> cleanup) =>
        _ = RunCleanupSafelyAsync(cleanup);

    private async Task RunCleanupSafelyAsync(Func<Task> cleanup)
    {
        try
        {
            await cleanup();
        }
        catch (Exception exception) when (IsIgnorableBackgroundFailure(exception))
        {
        }
        catch (Exception exception)
        {
            ReportBackgroundFailure(exception);
        }
    }

    private static bool IsIgnorableBackgroundFailure(Exception exception) =>
        exception is OperationCanceledException
            or ObjectDisposedException
            or TaskCanceledException
            or JSException;

    private void ReportBackgroundFailure(Exception exception)
    {
        if (_disposed)
        {
            return;
        }

        Diagnostics.ReportRecoverable(
            BackgroundMaintenanceOperation,
            Text(GoLiveText.Load.LoadMessage),
            exception,
            alreadyLogged: false);
    }
}
