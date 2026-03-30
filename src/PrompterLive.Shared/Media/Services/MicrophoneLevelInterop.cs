using Microsoft.JSInterop;

namespace PrompterLive.Shared.Services;

public sealed class MicrophoneLevelInterop(IJSRuntime jsRuntime)
{
    private const string StartMonitorIdentifier = "PrompterLive.media.startMicrophoneLevelMonitor";
    private const string StopMonitorIdentifier = "PrompterLive.media.stopMicrophoneLevelMonitor";

    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public Task StartAsync(string elementId, string deviceId)
    {
        return _jsRuntime.InvokeVoidAsync(StartMonitorIdentifier, elementId, deviceId).AsTask();
    }

    public Task StopAsync(string elementId)
    {
        return _jsRuntime.InvokeVoidAsync(StopMonitorIdentifier, elementId).AsTask();
    }
}
