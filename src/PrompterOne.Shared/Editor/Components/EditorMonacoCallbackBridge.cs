using Microsoft.JSInterop;
using PrompterOne.Shared.Services.Editor;

namespace PrompterOne.Shared.Components.Editor;

public sealed class EditorMonacoCallbackBridge(
    Func<string, Task> onHistoryRequested,
    Func<string, Task> onTextChanged,
    Func<EditorSelectionCallbackPayload, Task> onSelectionChanged)
{
    [JSInvokable(EditorMonacoInteropMethodNames.NotifyHistoryRequested)]
    public Task NotifyHistoryRequested(string command) =>
        onHistoryRequested(command ?? string.Empty);

    [JSInvokable(EditorMonacoInteropMethodNames.NotifyTextChanged)]
    public Task NotifyTextChanged(string text) =>
        onTextChanged(text ?? string.Empty);

    [JSInvokable(EditorMonacoInteropMethodNames.NotifySelectionChanged)]
    public Task NotifySelectionChanged(
        int start,
        int end,
        int line,
        int column,
        double toolbarTop,
        double toolbarLeft,
        bool dismissMenus) =>
        onSelectionChanged(new EditorSelectionCallbackPayload(start, end, line, column, toolbarTop, toolbarLeft, dismissMenus));
}

public sealed record EditorSelectionCallbackPayload(
    int Start,
    int End,
    int Line,
    int Column,
    double ToolbarTop,
    double ToolbarLeft,
    bool DismissMenus);
