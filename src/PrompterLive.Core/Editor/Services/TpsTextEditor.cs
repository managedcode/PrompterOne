using PrompterLive.Core.Models.Editor;

namespace PrompterLive.Core.Services.Editor;

public sealed class TpsTextEditor
{
    private static readonly IReadOnlyList<(string OpeningToken, string ClosingToken)> ColorTokenPairs =
    [
        ("[red]", "[/red]"),
        ("[green]", "[/green]"),
        ("[blue]", "[/blue]"),
        ("[yellow]", "[/yellow]"),
        ("[orange]", "[/orange]"),
        ("[purple]", "[/purple]"),
        ("[cyan]", "[/cyan]"),
        ("[magenta]", "[/magenta]"),
        ("[pink]", "[/pink]"),
        ("[teal]", "[/teal]"),
        ("[gray]", "[/gray]")
    ];

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

    public EditorTextMutationResult ClearColorFormatting(string? text, EditorSelectionRange selection)
    {
        var safeText = text ?? string.Empty;
        var range = NormalizeSelection(selection, safeText.Length);
        var selectedText = safeText[range.OrderedStart..range.OrderedEnd];
        var cleanedSelection = RemoveColorTokens(selectedText);

        if (!string.Equals(cleanedSelection, selectedText, StringComparison.Ordinal))
        {
            var updatedText = ReplaceRange(safeText, range, cleanedSelection);
            var end = range.OrderedStart + cleanedSelection.Length;
            return new EditorTextMutationResult(updatedText, new EditorSelectionRange(range.OrderedStart, end));
        }

        foreach (var (openingToken, closingToken) in ColorTokenPairs)
        {
            if (!TryFindEnclosingTokenRange(safeText, range, openingToken, closingToken, out var tokenRange))
            {
                continue;
            }

            var enclosedText = safeText[(tokenRange.Start + openingToken.Length)..(tokenRange.End - closingToken.Length)];
            var updatedText = ReplaceRange(safeText, tokenRange, enclosedText);
            var selectionLength = range.OrderedEnd - range.OrderedStart;
            var selectionStart = Math.Max(tokenRange.Start, range.OrderedStart - openingToken.Length);
            var selectionEnd = selectionStart + Math.Max(0, selectionLength);
            return new EditorTextMutationResult(updatedText, new EditorSelectionRange(selectionStart, selectionEnd));
        }

        return new EditorTextMutationResult(safeText, range);
    }

    private static EditorSelectionRange NormalizeSelection(EditorSelectionRange selection, int textLength)
    {
        var start = Math.Clamp(selection.OrderedStart, 0, textLength);
        var end = Math.Clamp(selection.OrderedEnd, 0, textLength);
        return new EditorSelectionRange(start, end);
    }

    private static string RemoveColorTokens(string text)
    {
        var cleaned = text;
        foreach (var (openingToken, closingToken) in ColorTokenPairs)
        {
            cleaned = cleaned.Replace(openingToken, string.Empty, StringComparison.Ordinal);
            cleaned = cleaned.Replace(closingToken, string.Empty, StringComparison.Ordinal);
        }

        return cleaned;
    }

    private static string ReplaceRange(string text, EditorSelectionRange selection, string replacement)
    {
        return string.Concat(
            text[..selection.OrderedStart],
            replacement,
            text[selection.OrderedEnd..]);
    }

    private static bool TryFindEnclosingTokenRange(
        string text,
        EditorSelectionRange selection,
        string openingToken,
        string closingToken,
        out EditorSelectionRange tokenRange)
    {
        var searchStart = text.Length == 0
            ? -1
            : Math.Min(selection.OrderedStart, text.Length - 1);

        if (searchStart < 0)
        {
            tokenRange = EditorSelectionRange.Empty;
            return false;
        }

        var openingIndex = text.LastIndexOf(openingToken, searchStart, StringComparison.Ordinal);
        if (openingIndex < 0)
        {
            tokenRange = EditorSelectionRange.Empty;
            return false;
        }

        var closingSearchStart = Math.Max(selection.OrderedEnd, openingIndex + openingToken.Length);
        var closingIndex = text.IndexOf(closingToken, closingSearchStart, StringComparison.Ordinal);
        if (closingIndex < 0)
        {
            tokenRange = EditorSelectionRange.Empty;
            return false;
        }

        var innerStart = openingIndex + openingToken.Length;
        var innerEnd = closingIndex;
        if (selection.OrderedStart < innerStart || selection.OrderedEnd > innerEnd)
        {
            tokenRange = EditorSelectionRange.Empty;
            return false;
        }

        tokenRange = new EditorSelectionRange(openingIndex, closingIndex + closingToken.Length);
        return true;
    }
}
