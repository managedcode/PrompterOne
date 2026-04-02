using Microsoft.JSInterop;
using PrompterOne.Shared.Components.GoLive;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Services;

namespace PrompterOne.Shared.Pages;

public partial class GoLivePage
{
    private DotNetObjectReference<MicrophoneLevelObserver>? _microphoneLevelObserver;
    private string? _monitoredMicrophoneDeviceId;
    private int _primaryMicrophoneLevelPercent;

    private bool ShouldMonitorPrimaryMicrophone =>
        _activeStudioTab == GoLiveStudioTab.Audio
        && !string.IsNullOrWhiteSpace(MediaSceneService.State.PrimaryMicrophoneId);

    private async Task SyncPrimaryMicrophoneMonitorAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (!ShouldMonitorPrimaryMicrophone)
        {
            await StopPrimaryMicrophoneMonitorAsync();
            return;
        }

        var nextDeviceId = MediaSceneService.State.PrimaryMicrophoneId!;
        if (string.Equals(_monitoredMicrophoneDeviceId, nextDeviceId, StringComparison.Ordinal))
        {
            return;
        }

        await StopPrimaryMicrophoneMonitorAsync();
        _microphoneLevelObserver ??= DotNetObjectReference.Create(new MicrophoneLevelObserver(HandlePrimaryMicrophoneLevelChangedAsync));
        _monitoredMicrophoneDeviceId = nextDeviceId;

        try
        {
            await MicrophoneLevelInterop.StartAsync(
                UiDomIds.GoLive.MicrophoneLevelMonitor,
                nextDeviceId,
                _microphoneLevelObserver);
        }
        catch
        {
            _monitoredMicrophoneDeviceId = null;
            await ResetPrimaryMicrophoneLevelAsync();
        }
    }

    private async Task StopPrimaryMicrophoneMonitorAsync()
    {
        if (string.IsNullOrWhiteSpace(_monitoredMicrophoneDeviceId))
        {
            await ResetPrimaryMicrophoneLevelAsync();
            return;
        }

        try
        {
            await MicrophoneLevelInterop.StopAsync(UiDomIds.GoLive.MicrophoneLevelMonitor);
        }
        catch
        {
        }
        finally
        {
            _monitoredMicrophoneDeviceId = null;
            await ResetPrimaryMicrophoneLevelAsync();
        }
    }

    private void DisposePrimaryMicrophoneObserver()
    {
        _microphoneLevelObserver?.Dispose();
        _microphoneLevelObserver = null;
        _monitoredMicrophoneDeviceId = null;
        _primaryMicrophoneLevelPercent = 0;
    }

    private Task HandlePrimaryMicrophoneLevelChangedAsync(int levelPercent)
    {
        var nextLevel = Math.Clamp(levelPercent, 0, 100);
        if (_disposed)
        {
            _primaryMicrophoneLevelPercent = nextLevel;
            return Task.CompletedTask;
        }

        if (_primaryMicrophoneLevelPercent == nextLevel)
        {
            return Task.CompletedTask;
        }

        _primaryMicrophoneLevelPercent = nextLevel;
        return InvokeAsync(StateHasChanged);
    }

    private Task ResetPrimaryMicrophoneLevelAsync() => HandlePrimaryMicrophoneLevelChangedAsync(0);
}
