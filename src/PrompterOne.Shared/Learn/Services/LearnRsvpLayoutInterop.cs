using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace PrompterOne.Shared.Services;

public sealed class LearnRsvpLayoutInterop(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public ValueTask SyncLayoutAsync(ElementReference row) =>
        _jsRuntime.InvokeVoidAsync(LearnRsvpLayoutInteropMethodNames.SyncLayout, row);
}
