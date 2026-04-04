namespace PrompterOne.Core.Models.Editor;

public sealed record EditorDroppedScriptMergeResult(
    string Text,
    EditorSelectionRange Selection,
    bool ReplacedExistingText);
