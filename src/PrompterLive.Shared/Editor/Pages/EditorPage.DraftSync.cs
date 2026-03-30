using PrompterLive.Core.Services.Workspace;

namespace PrompterLive.Shared.Pages;

public partial class EditorPage
{
    private void QueueDraftStateSync()
    {
        CancelDraftStateSync();
        _draftSyncCancellationSource = new CancellationTokenSource();
        _ = RunDraftStateSyncAsync(_draftSyncCancellationSource.Token);
    }

    private void RefreshDraftViewFromSource()
    {
        var persistedText = BuildPersistedDocument(_sourceText);

        try
        {
            var scriptData = TpsParser.ParseTps(persistedText);
            _segments = OutlineBuilder.Build(scriptData, _sourceText, 0);
            _errorMessage = null;
            StageDraftText(persistedText, null);
        }
        catch (Exception exception)
        {
            _segments = [];
            _errorMessage = exception.Message;
            StageDraftText(persistedText, exception.Message);
        }

        UpdateSyntaxDiagnostics();
        RefreshSelectionState();
    }

    private async Task RunDraftStateSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(DraftSyncDelayMilliseconds, cancellationToken);
            await InvokeAsync(() => UpdateDraftStateAsync(_sourceText, persistDocument: false, cancellationToken));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CancelDraftStateSync()
    {
        if (_draftSyncCancellationSource is null)
        {
            return;
        }

        _draftSyncCancellationSource.Cancel();
        _draftSyncCancellationSource.Dispose();
        _draftSyncCancellationSource = null;
    }

    private void StageDraftText(string persistedText, string? errorMessage)
    {
        if (SessionService is not ScriptSessionService sessionService)
        {
            return;
        }

        sessionService.StageDraftText(
            _screenTitle,
            persistedText,
            SessionService.State.DocumentName,
            SessionService.State.ScriptId,
            errorMessage);
    }
}
