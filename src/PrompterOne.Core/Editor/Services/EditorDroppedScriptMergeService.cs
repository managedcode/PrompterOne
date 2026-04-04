using PrompterOne.Core.Models.Editor;

namespace PrompterOne.Core.Services.Editor;

public sealed class EditorDroppedScriptMergeService
{
    private const string DocumentSeparator = "\n\n";

    public EditorDroppedScriptMergeResult Merge(string? existingText, IReadOnlyList<string> importedBodies)
    {
        var normalizedBodies = importedBodies
            .Select(NormalizeImportedBody)
            .Where(static body => !string.IsNullOrWhiteSpace(body))
            .ToArray();

        if (normalizedBodies.Length == 0)
        {
            var currentText = existingText ?? string.Empty;
            var currentCaret = currentText.Length;
            return new EditorDroppedScriptMergeResult(
                currentText,
                new EditorSelectionRange(currentCaret, currentCaret),
                ReplacedExistingText: string.IsNullOrWhiteSpace(existingText));
        }

        if (string.IsNullOrWhiteSpace(existingText))
        {
            var replacementText = string.Join(DocumentSeparator, normalizedBodies);
            var replacementCaret = replacementText.Length;
            return new EditorDroppedScriptMergeResult(
                replacementText,
                new EditorSelectionRange(replacementCaret, replacementCaret),
                ReplacedExistingText: true);
        }

        var appendedText = string.Concat(
            existingText.TrimEnd(),
            DocumentSeparator,
            string.Join(DocumentSeparator, normalizedBodies));
        var appendedCaret = appendedText.Length;
        return new EditorDroppedScriptMergeResult(
            appendedText,
            new EditorSelectionRange(appendedCaret, appendedCaret),
            ReplacedExistingText: false);
    }

    private static string NormalizeImportedBody(string? body) =>
        body?.Trim() ?? string.Empty;
}
