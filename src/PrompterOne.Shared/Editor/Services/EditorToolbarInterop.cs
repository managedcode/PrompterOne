using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PrompterOne.Shared.Services.Editor;

public sealed class EditorToolbarInterop(IJSRuntime jsRuntime) : IDisposable, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private Task<IJSObjectReference?>? _moduleTask;

    public async ValueTask ScrollByAsync(ElementReference host, double delta)
    {
        var module = await GetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync(
            EditorToolbarInteropMethodNames.ScrollBy,
            host,
            delta);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is null)
        {
            return;
        }

        var module = await _moduleTask;
        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }

    public void Dispose()
    {
    }

    private Task<IJSObjectReference?> GetModuleAsync() =>
        _moduleTask ??= ImportModuleAsync();

    private async Task<IJSObjectReference?> ImportModuleAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<IJSObjectReference>(
                EditorToolbarInteropMethodNames.JSImportMethodName,
                EditorToolbarInteropMethodNames.ModulePath);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (JSException)
        {
            return null;
        }
    }
}
