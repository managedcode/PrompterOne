namespace PrompterOne.Core.Models.Editor;

public sealed record EditorTextMutationResult(
    string Text,
    EditorSelectionRange Selection);
