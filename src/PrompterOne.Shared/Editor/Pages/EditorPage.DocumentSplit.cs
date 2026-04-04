using PrompterOne.Core.Models.Documents;
using PrompterOne.Core.Models.Editor;
using PrompterOne.Core.Models.Workspace;
using PrompterOne.Shared.Services;

namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private const string SplitDocumentNameExtension = ".tps";
    private const string SplitDocumentNameSeparator = "-";
    private const string SplitDocumentNameToken = "split";
    private const int MinimumSplitDocumentCount = 2;

    private Task OnSplitRequestedAsync(TpsDocumentSplitMode mode)
    {
        var persistedText = BuildPersistedDocument(_sourceText);
        var splitDocuments = DocumentSplitService.Split(persistedText, mode);
        if (splitDocuments.Count < MinimumSplitDocumentCount)
        {
            Diagnostics.ReportRecoverable(
                SplitDraftOperation,
                SplitDraftNoMatchesMessage,
                ResolveSplitRequirementDetail(mode));
            _splitStatusMessage = null;
            StateHasChanged();
            return Task.CompletedTask;
        }

        return Diagnostics.RunAsync(
            SplitDraftOperation,
            SplitDraftMessage,
            async () =>
            {
                var sourceDocument = await ResolveCurrentScriptDocumentAsync();
                var baseDocumentName = ResolveBaseDocumentName(sourceDocument);

                foreach (var splitDocument in splitDocuments)
                {
                    _ = await ScriptRepository.SaveAsync(
                        splitDocument.Title,
                        splitDocument.Text,
                        documentName: BuildSplitDocumentName(baseDocumentName, splitDocument),
                        existingId: null,
                        folderId: sourceDocument?.FolderId);
                }

                _splitStatusMessage = BuildSplitStatusMessage(splitDocuments.Count, mode);
                await InvokeAsync(StateHasChanged);
            });
    }

    private Task<StoredScriptDocument?> ResolveCurrentScriptDocumentAsync()
    {
        if (string.IsNullOrWhiteSpace(ScriptId))
        {
            return Task.FromResult<StoredScriptDocument?>(null);
        }

        return ScriptRepository.GetAsync(ScriptId);
    }

    private static string ResolveBaseDocumentName(StoredScriptDocument? sourceDocument) =>
        string.IsNullOrWhiteSpace(sourceDocument?.DocumentName)
            ? ScriptWorkspaceState.UntitledScriptDocumentName
            : sourceDocument.DocumentName;

    private static string BuildSplitDocumentName(string baseDocumentName, TpsDocumentSplitDocument splitDocument)
    {
        var baseStem = BrowserStorageSlugifier.Slugify(Path.GetFileNameWithoutExtension(baseDocumentName));
        var childStem = BrowserStorageSlugifier.Slugify(splitDocument.Title);
        return string.Join(
                   SplitDocumentNameSeparator,
                   baseStem,
                   SplitDocumentNameToken,
                   splitDocument.Sequence.ToString("00", System.Globalization.CultureInfo.InvariantCulture),
                   childStem)
               + SplitDocumentNameExtension;
    }

    private static string BuildSplitStatusMessage(int count, TpsDocumentSplitMode mode)
    {
        var headingLabel = mode == TpsDocumentSplitMode.TopLevelHeading ? "#" : "##";
        var noun = count == 1 ? "script" : "scripts";
        return $"{count} {noun} created from {headingLabel} headings.";
    }

    private static string ResolveSplitRequirementDetail(TpsDocumentSplitMode mode) =>
        mode == TpsDocumentSplitMode.TopLevelHeading
            ? "Need at least two # headings to split this draft."
            : "Need at least two ## headings to split this draft.";
}
