using Microsoft.AspNetCore.Components;
using PrompterLive.Shared.Rendering;

namespace PrompterLive.Shared.Components.Editor;

public partial class EditorSourcePanel
{
    private ElementReference _overlayRef;
    private ElementReference _textareaRef;
    private EditorSelectionViewModel _domSelection = EditorSelectionViewModel.Empty;

    [Parameter] public string? ErrorMessage { get; set; }

    [Parameter] public EventCallback<EditorCommandRequest> OnCommandRequested { get; set; }

    [Parameter] public EventCallback<EditorSelectionViewModel> OnSelectionChanged { get; set; }

    [Parameter] public EventCallback<string> OnTextChanged { get; set; }

    [Parameter] public EditorSelectionViewModel Selection { get; set; } = EditorSelectionViewModel.Empty;

    [Parameter] public EditorStatusViewModel Status { get; set; } = new(1, 1, "Actor", 140, 0, 0, 0, "0:00", "1.0");

    [Parameter] public string Text { get; set; } = string.Empty;

    protected MarkupString HighlightMarkup => TpsSourceHighlighter.Render(Text);

    protected string FloatingBarStyle => $"left:{Selection.ToolbarLeft}px; top:{Selection.ToolbarTop}px;";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await SafeSyncScrollAsync();

        if (Selection.Range != _domSelection.Range)
        {
            try
            {
                _domSelection = await Interop.SetSelectionAsync(_textareaRef, Selection.Range.Start, Selection.Range.End);
            }
            catch
            {
                _domSelection = Selection;
            }
        }
    }

    public async Task FocusRangeAsync(int start, int end)
    {
        try
        {
            _domSelection = await Interop.SetSelectionAsync(_textareaRef, start, end);
            await OnSelectionChanged.InvokeAsync(_domSelection);
            StateHasChanged();
        }
        catch
        {
        }
    }

    protected Task RequestInsertAsync(string token, int? caretOffset = null) =>
        OnCommandRequested.InvokeAsync(new EditorCommandRequest(EditorCommandKind.Insert, token, CaretOffset: caretOffset));

    protected Task RequestWrapAsync(string openingToken, string closingToken, string placeholder = "text") =>
        OnCommandRequested.InvokeAsync(new EditorCommandRequest(EditorCommandKind.Wrap, openingToken, closingToken, placeholder));

    private async Task OnScrollAsync()
    {
        await SafeSyncScrollAsync();
        await RefreshSelectionAsync();
    }

    private async Task OnSelectionInteractionAsync()
    {
        await RefreshSelectionAsync();
    }

    private async Task OnSourceInputAsync(ChangeEventArgs args)
    {
        await OnTextChanged.InvokeAsync(args.Value?.ToString() ?? string.Empty);
        await RefreshSelectionAsync();
    }

    private async Task RefreshSelectionAsync()
    {
        try
        {
            _domSelection = await Interop.GetSelectionAsync(_textareaRef);
            await OnSelectionChanged.InvokeAsync(_domSelection);
            StateHasChanged();
        }
        catch
        {
        }
    }

    private async Task SafeSyncScrollAsync()
    {
        try
        {
            await Interop.SyncScrollAsync(_textareaRef, _overlayRef);
        }
        catch
        {
        }
    }
}
