using System.Globalization;
using PrompterOne.Core.Models.Documents;
using PrompterOne.Shared.Components.Editor;
using PrompterOne.Shared.Services.Editor;

namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private const string LocalRevisionTimestampFormat = "yyyy-MM-dd HH:mm:ss";

    private async Task LoadEditorFileWorkflowAsync(CancellationToken cancellationToken = default)
    {
        _fileStorageSettings = await BrowserFileStorageStore.LoadSettingsAsync(cancellationToken);
        await LoadLocalHistoryAsync(SessionService.State.ScriptId, cancellationToken);
    }

    private async Task LoadLocalHistoryAsync(string? scriptId, CancellationToken cancellationToken = default)
    {
        var revisions = await EditorLocalRevisionStore.LoadAsync(scriptId, cancellationToken);
        ApplyLocalHistory(revisions);
    }

    private async Task CaptureLocalRevisionAsync(
        StoredScriptDocument savedDocument,
        CancellationToken cancellationToken)
    {
        _lastLocalSaveAt = savedDocument.UpdatedAt;
        if (!_fileStorageSettings.FileBackupCopiesEnabled)
        {
            return;
        }

        var revisions = await EditorLocalRevisionStore.RecordAsync(
            new EditorLocalRevisionDraft(
                savedDocument.Id,
                savedDocument.Title,
                savedDocument.DocumentName,
                savedDocument.Text,
                savedDocument.UpdatedAt),
            cancellationToken);

        ApplyLocalHistory(revisions);
    }

    private async Task OnLocalHistoryRestoreRequestedAsync(string revisionId)
    {
        var revision = await EditorLocalRevisionStore.GetAsync(SessionService.State.ScriptId, revisionId);
        if (revision is null)
        {
            return;
        }

        _selection = EditorSelectionViewModel.Empty;
        _ = TryImportFrontMatterFromSource(revision.PersistedText, out var bodyText);
        _history.TryRecord(bodyText, _selection.Range);
        await PersistDraftAsync(bodyText, revision.DocumentName);
        await LoadLocalHistoryAsync(SessionService.State.ScriptId);
    }

    private void ApplyLocalHistory(IReadOnlyList<EditorLocalRevisionRecord> revisions)
    {
        _localHistory = revisions
            .Select(CreateLocalRevisionViewModel)
            .ToList();

        _lastLocalSaveAt = revisions.Count > 0
            ? revisions[0].SavedAt
            : null;
    }

    private static EditorLocalRevisionViewModel CreateLocalRevisionViewModel(EditorLocalRevisionRecord revision) =>
        new(
            revision.Id,
            revision.SavedAt.LocalDateTime.ToString(LocalRevisionTimestampFormat, CultureInfo.InvariantCulture),
            revision.Title,
            revision.DocumentName);
}
