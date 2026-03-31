using Microsoft.JSInterop;

namespace PrompterLive.Shared.Services;

public sealed class TeleprompterReaderInterop(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public ValueTask<double?> MeasureClusterOffsetAsync(
        string stageId,
        string textId,
        string targetWordId,
        int focalPointPercent) =>
        _jsRuntime.InvokeAsync<double?>(
            TeleprompterReaderInteropMethodNames.MeasureClusterOffset,
            stageId,
            textId,
            targetWordId,
            focalPointPercent);
}
