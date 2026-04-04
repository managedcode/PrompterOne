using PrompterOne.Core.Models.Editor;
using PrompterOne.Core.Services.Editor;

namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private Task OnFilesDroppedAsync(EditorDroppedFilesRequest request)
    {
        if (request.Files.Count == 0)
        {
            if (request.RejectedFileNames.Count > 0)
            {
                Diagnostics.ReportRecoverable(DropScriptOperation, DropScriptMessage, DropScriptUnsupportedDetail);
            }

            return Task.CompletedTask;
        }

        Diagnostics.ClearRecoverable(DropScriptOperation);
        return Diagnostics.RunAsync(
            DropScriptOperation,
            DropScriptMessage,
            () => ImportDroppedFilesAsync(request));
    }

    private async Task ImportDroppedFilesAsync(EditorDroppedFilesRequest request)
    {
        var droppedDocuments = request.Files
            .Select(CreateDroppedEditorDocument)
            .ToArray();
        var importedBodies = droppedDocuments
            .Select(static document => document.BodyText)
            .Where(static body => !string.IsNullOrWhiteSpace(body))
            .ToArray();

        if (importedBodies.Length == 0)
        {
            Diagnostics.ReportRecoverable(DropScriptOperation, DropScriptMessage, DropScriptUnsupportedDetail);
            return;
        }

        var mergeResult = DroppedScriptMergeService.Merge(_sourceText, importedBodies);
        var replacementDocument = droppedDocuments[0];
        var documentNameOverride = mergeResult.ReplacedExistingText
            ? replacementDocument.DocumentName
            : null;

        if (mergeResult.ReplacedExistingText)
        {
            ApplyDroppedReplacementMetadata(replacementDocument);
        }

        await ApplyMutationAsync(mergeResult.Text, mergeResult.Selection, documentNameOverride);
    }

    private DroppedEditorDocument CreateDroppedEditorDocument(EditorDroppedFile droppedFile)
    {
        var descriptor = ScriptImportDescriptorService.Build(droppedFile.FileName, droppedFile.Text);
        var document = _frontMatterService.Parse(descriptor.Text);
        return new DroppedEditorDocument(
            descriptor.Title,
            descriptor.DocumentName,
            document.Metadata,
            document.Body,
            document.BodyStartIndex > 0);
    }

    private void ApplyDroppedReplacementMetadata(DroppedEditorDocument document)
    {
        if (document.HasFrontMatter)
        {
            ApplyImportedMetadata(document.Metadata);
            if (!document.Metadata.ContainsKey(TpsFrontMatterDocumentService.MetadataKeys.Title))
            {
                ApplyDroppedTitle(document.Title);
            }

            return;
        }

        ApplyDroppedTitle(document.Title);
    }

    private void ApplyDroppedTitle(string title)
    {
        _screenTitle = title;
        Shell.ShowEditor(_screenTitle, SessionService.State.ScriptId);
    }

    private sealed record DroppedEditorDocument(
        string Title,
        string DocumentName,
        IReadOnlyDictionary<string, string> Metadata,
        string BodyText,
        bool HasFrontMatter);
}
