using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PrompterLive.Core.Models.Editor;
using PrompterLive.Shared.Components.Editor;

namespace PrompterLive.Shared.Services.Editor;

public sealed class EditorInterop(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public ValueTask SyncScrollAsync(ElementReference textarea, ElementReference overlay) =>
        _jsRuntime.InvokeVoidAsync("PrompterLive.editor.syncScroll", textarea, overlay);

    public ValueTask BindHistoryShortcutsAsync<TValue>(
        ElementReference textarea,
        DotNetObjectReference<TValue> callbackReference)
        where TValue : class =>
        _jsRuntime.InvokeVoidAsync("PrompterLive.editor.bindHistoryShortcuts", textarea, callbackReference);

    public async Task<EditorSelectionViewModel> GetSelectionAsync(ElementReference textarea)
    {
        var result = await _jsRuntime.InvokeAsync<EditorSelectionInteropResult?>(
            "PrompterLive.editor.getSelectionState",
            textarea);

        if (result is null)
        {
            return EditorSelectionViewModel.Empty;
        }

        return new EditorSelectionViewModel(
            new EditorSelectionRange(result.Start, result.End),
            result.Line,
            result.Column,
            result.ToolbarTop,
            result.ToolbarLeft);
    }

    public async Task<EditorSelectionViewModel> SetSelectionAsync(ElementReference textarea, int start, int end)
    {
        var result = await _jsRuntime.InvokeAsync<EditorSelectionInteropResult?>(
            "PrompterLive.editor.setSelection",
            textarea,
            start,
            end);

        if (result is null)
        {
            return EditorSelectionViewModel.Empty;
        }

        return new EditorSelectionViewModel(
            new EditorSelectionRange(result.Start, result.End),
            result.Line,
            result.Column,
            result.ToolbarTop,
            result.ToolbarLeft);
    }

    public ValueTask UnbindHistoryShortcutsAsync(ElementReference textarea) =>
        _jsRuntime.InvokeVoidAsync("PrompterLive.editor.unbindHistoryShortcuts", textarea);

    private sealed record EditorSelectionInteropResult(
        int Start,
        int End,
        int Line,
        int Column,
        double ToolbarTop,
        double ToolbarLeft);
}
