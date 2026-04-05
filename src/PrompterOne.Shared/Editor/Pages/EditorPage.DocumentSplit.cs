using PrompterOne.Core.Models.Documents;
using PrompterOne.Core.Models.Editor;
using PrompterOne.Core.Models.Workspace;
using PrompterOne.Shared.Components.Editor;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Services;

namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private const int SplitFeedbackPreviewLimit = 3;
    private const string SplitDocumentNameExtension = ".tps";
    private const string SplitDocumentNameSeparator = "-";
    private const string SplitDocumentNameToken = "split";
    private const string SplitFeedbackActionLabel = "Open In Library";
    private const string SplitFeedbackSavedToLibraryMessage = "New scripts were added to Library. Open them there when you are ready.";
    private const string SplitFeedbackSavedToSiblingFolderMessage = "New scripts were added next to this draft in the same Library folder.";
    private const string SplitFeedbackSavedToSiblingMessage = "New scripts were added next to this draft in Library.";
    private const string SplitFeedbackDraftRemainsOpenMessage = "This draft stayed open here so you can keep editing.";
    private const string SplitFeedbackTitle = "Split complete";
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
            _splitFeedback = null;
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

                _splitFeedback = BuildSplitFeedback(splitDocuments, sourceDocument, mode);
                await InvokeAsync(StateHasChanged);
            });
    }

    private void OnOpenLibraryRequested()
    {
        Navigation.NavigateTo(AppRoutes.Library);
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

    private static EditorSplitFeedbackViewModel BuildSplitFeedback(
        IReadOnlyList<TpsDocumentSplitDocument> splitDocuments,
        StoredScriptDocument? sourceDocument,
        TpsDocumentSplitMode mode)
    {
        var previewTitles = splitDocuments
            .Take(SplitFeedbackPreviewLimit)
            .Select(document => document.Title)
            .ToArray();
        var additionalCount = Math.Max(0, splitDocuments.Count - previewTitles.Length);

        return new EditorSplitFeedbackViewModel(
            Title: SplitFeedbackTitle,
            Summary: BuildSplitSummary(splitDocuments.Count, mode),
            HeadingBadge: BuildSplitHeadingBadge(mode),
            DestinationNote: BuildSplitDestinationNote(sourceDocument),
            DraftNote: SplitFeedbackDraftRemainsOpenMessage,
            OpenLibraryLabel: SplitFeedbackActionLabel,
            CreatedTitles: previewTitles,
            AdditionalCount: additionalCount);
    }

    private static string ResolveSplitRequirementDetail(TpsDocumentSplitMode mode) =>
        mode == TpsDocumentSplitMode.TopLevelHeading
            ? "Need at least two # headings to split this draft."
            : "Need at least two ## headings to split this draft.";

    private static string BuildSplitDestinationNote(StoredScriptDocument? sourceDocument)
    {
        if (sourceDocument is null)
        {
            return SplitFeedbackSavedToLibraryMessage;
        }

        return string.IsNullOrWhiteSpace(sourceDocument.FolderId)
            ? SplitFeedbackSavedToSiblingMessage
            : SplitFeedbackSavedToSiblingFolderMessage;
    }

    private static string BuildSplitHeadingBadge(TpsDocumentSplitMode mode) =>
        $"{ResolveHeadingLabel(mode)} headings";

    private static string BuildSplitSummary(int count, TpsDocumentSplitMode mode)
    {
        var noun = count == 1 ? "script" : "scripts";
        return $"{count} new {noun} created.";
    }

    private static string ResolveHeadingLabel(TpsDocumentSplitMode mode) =>
        mode == TpsDocumentSplitMode.TopLevelHeading ? "#" : "##";
}
