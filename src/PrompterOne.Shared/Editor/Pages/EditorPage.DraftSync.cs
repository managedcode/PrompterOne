using PrompterOne.Core.Services.Workspace;
using PrompterOne.Shared.Services.Editor;

namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private void RefreshDraftViewFromSource(string? documentNameOverride = null)
    {
        var persistedText = BuildPersistedDocument(_sourceText);

        try
        {
            var scriptData = TpsScriptDataFactory.Build(persistedText);
            _segments = OutlineBuilder.Build(scriptData, _sourceText, 0);
            _errorMessage = null;
            UpdateDraftMetrics(scriptData);
            StageDraftText(persistedText, null, documentNameOverride);
        }
        catch (Exception exception)
        {
            _segments = [];
            _errorMessage = exception.Message;
            UpdateDraftMetrics((PrompterOne.Core.Models.Documents.ScriptData?)null);
            StageDraftText(persistedText, exception.Message, documentNameOverride);
        }

        UpdateSyntaxDiagnostics();
        RefreshSelectionState();
    }

    private void StageDraftText(string persistedText, string? errorMessage, string? documentNameOverride = null)
    {
        if (SessionService is not ScriptSessionService sessionService)
        {
            return;
        }

        sessionService.StageDraftText(
            _screenTitle,
            persistedText,
            string.IsNullOrWhiteSpace(documentNameOverride) ? SessionService.State.DocumentName : documentNameOverride,
            SessionService.State.ScriptId,
            errorMessage);
    }

    private void UpdateDraftMetrics(PrompterOne.Core.Models.Workspace.ScriptWorkspaceState state)
    {
        _draftMetrics = new EditorDraftMetrics(state.WordCount, state.EstimatedDuration);
    }

    private void UpdateDraftMetrics(PrompterOne.Core.Models.Documents.ScriptData? scriptData)
    {
        _draftMetrics = EditorDraftMetricsCalculator.Calculate(scriptData);
    }
}
