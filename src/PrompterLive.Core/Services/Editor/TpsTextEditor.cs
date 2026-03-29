using PrompterLive.Core.Models.Editor;

namespace PrompterLive.Core.Services.Editor;

public sealed class TpsTextEditor
{
    public EditorTextMutationResult WrapSelection(
        string? text,
        EditorSelectionRange selection,
        string openingToken,
        string closingToken,
        string placeholderText)
    {
        var safeText = text ?? string.Empty;
        var range = NormalizeSelection(selection, safeText.Length);
        var innerText = range.HasSelection
            ? safeText[range.OrderedStart..range.OrderedEnd]
            : placeholderText;
        var replacement = string.Concat(openingToken, innerText, closingToken);
        var updatedText = ReplaceRange(safeText, range, replacement);
        var selectionStart = range.OrderedStart + openingToken.Length;
        var selectionEnd = selectionStart + innerText.Length;

        return new EditorTextMutationResult(
            updatedText,
            new EditorSelectionRange(selectionStart, selectionEnd));
    }

    public EditorTextMutationResult InsertAtSelection(
        string? text,
        EditorSelectionRange selection,
        string insertionText,
        int? caretOffset = null)
    {
        var safeText = text ?? string.Empty;
        var range = NormalizeSelection(selection, safeText.Length);
        var updatedText = ReplaceRange(safeText, range, insertionText);
        var caretIndex = range.OrderedStart + (caretOffset ?? insertionText.Length);

        return new EditorTextMutationResult(
            updatedText,
            new EditorSelectionRange(caretIndex, caretIndex));
    }

    private static EditorSelectionRange NormalizeSelection(EditorSelectionRange selection, int textLength)
    {
        var start = Math.Clamp(selection.OrderedStart, 0, textLength);
        var end = Math.Clamp(selection.OrderedEnd, 0, textLength);
        return new EditorSelectionRange(start, end);
    }

    private static string ReplaceRange(string text, EditorSelectionRange selection, string replacement)
    {
        return string.Concat(
            text[..selection.OrderedStart],
            replacement,
            text[selection.OrderedEnd..]);
    }
}
