using PrompterOne.Core.Models.Editor;

namespace PrompterOne.Shared.Components.Editor;

public partial class EditorSourcePanel
{
    private Task HandleMonacoHistoryRequestedAsync(string command) =>
        RequestHistoryAsync(
            string.Equals(command, "redo", StringComparison.OrdinalIgnoreCase)
                ? EditorHistoryCommand.Redo
                : EditorHistoryCommand.Undo);

    private async Task HandleMonacoTextChangedAsync(string text)
    {
        var hadOpenToolbarMenu = HasOpenToolbarMenu;
        CloseToolbarPanels();

        _lastTypedText = text ?? string.Empty;
        _hasPendingLocalInputText = true;
        _visibleCanUndo = true;
        _visibleCanRedo = false;
        _skipNextRender = _surfaceInteropReady && !hadOpenToolbarMenu;
        await OnTextChanged.InvokeAsync(_lastTypedText);
    }

    private async Task HandleMonacoSelectionChangedAsync(EditorSelectionCallbackPayload payload)
    {
        var selection = new EditorSelectionViewModel(
            new EditorSelectionRange(payload.Start, payload.End),
            payload.Line,
            payload.Column,
            payload.ToolbarTop,
            payload.ToolbarLeft);

        if (payload.DismissMenus)
        {
            RequestFloatingBarReanchor();
            CloseToolbarPanels();
        }
        else if (!_floatingBarAnchor.HasSelection || selection.Range != Selection.Range)
        {
            RequestFloatingBarReanchor();
        }

        await OnSelectionChanged.InvokeAsync(selection);
        var shouldRender = payload.DismissMenus ||
                           selection.HasSelection ||
                           Selection.HasSelection ||
                           HasOpenToolbarMenu ||
                           _floatingBarAnchor.HasSelection;

        if (!shouldRender)
        {
            return;
        }

        _syncSurfaceAfterRender = selection.HasSelection || Selection.HasSelection;
        StateHasChanged();
    }
}
