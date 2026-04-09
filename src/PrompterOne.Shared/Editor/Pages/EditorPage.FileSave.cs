using PrompterOne.Core.Models.Workspace;
using PrompterOne.Core.Services.Editor;
using PrompterOne.Shared.Services;

namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private async Task HandleSaveFileRequestedAsync(CancellationToken cancellationToken)
    {
        CancelDraftAnalysis();
        CancelAutosave();
        var revision = PrepareDraftPersistence(_sourceText);

        await Diagnostics.RunAsync(
            SaveFileOperation,
            SaveFileMessage,
            async () =>
            {
                await PersistDraftStateCoreAsync(persistDocument: true, cancellationToken, revision);
                await FilePickerInterop.SaveTextAsync(
                    ResolveExportDocumentName(),
                    BuildPersistedDocument(_sourceText),
                    ScriptDocumentFileTypes.TextMimeType,
                    ScriptDocumentFileTypes.SavePickerDescription,
                    ScriptDocumentFileTypes.SaveSupportedFileNameSuffixes);
            },
            clearRecoverableOnSuccess: string.IsNullOrWhiteSpace(SessionService.State.ErrorMessage));
    }

    private string ResolveExportDocumentName()
    {
        var normalizedDocumentName = ScriptDocumentFileTypes.NormalizeFileName(SessionService.State.DocumentName);
        var supportedSuffix = ScriptDocumentFileTypes.ResolveSaveSupportedSuffix(normalizedDocumentName) ?? ScriptDocumentFileTypes.DefaultExtension;

        if (!string.IsNullOrWhiteSpace(normalizedDocumentName)
            && !string.Equals(
                normalizedDocumentName,
                ScriptWorkspaceState.UntitledScriptDocumentName,
                StringComparison.OrdinalIgnoreCase))
        {
            return normalizedDocumentName;
        }

        return string.Concat(BrowserStorageSlugifier.Slugify(_screenTitle), supportedSuffix);
    }
}
