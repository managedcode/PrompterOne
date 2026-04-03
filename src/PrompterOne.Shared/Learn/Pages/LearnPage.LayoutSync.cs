namespace PrompterOne.Shared.Pages;

public partial class LearnPage
{
    private const string HiddenLayoutStyleValue = "opacity:0;visibility:hidden;";
    private const string LayoutReadyFalseValue = "false";
    private const string LayoutReadyTrueValue = "true";

    private bool _isFocusLayoutReady;
    private TaskCompletionSource<bool>? _pendingFocusLayoutSyncCompletionSource;

    private string BuildLayoutReadyAttributeValue() => _isFocusLayoutReady
        ? LayoutReadyTrueValue
        : LayoutReadyFalseValue;

    private string? BuildPendingLayoutHiddenStyle() => _isFocusLayoutReady
        ? null
        : HiddenLayoutStyleValue;

    private void MarkFocusLayoutDirty()
    {
        _isFocusLayoutReady = false;
        _pendingFocusLayoutSyncCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private async Task AwaitPendingFocusLayoutSyncAsync()
    {
        var pendingSync = _pendingFocusLayoutSyncCompletionSource;
        if (pendingSync is null)
        {
            return;
        }

        await pendingSync.Task;
    }

    private void CompletePendingFocusLayoutSync(bool isReady)
    {
        _isFocusLayoutReady = isReady;
        _pendingFocusLayoutSyncCompletionSource?.TrySetResult(isReady);
    }
}
