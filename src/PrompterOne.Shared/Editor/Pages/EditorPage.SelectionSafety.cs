namespace PrompterOne.Shared.Pages;

public partial class EditorPage
{
    private bool CanApplyWrapCommands =>
        !_selection.HasSelection || !TextEditor.SelectionTouchesTagSyntax(_sourceText, _selection.Range);

    private bool CanRenderFloatingToolbar =>
        _selection.HasSelection && !TextEditor.SelectionTouchesTagSyntax(_sourceText, _selection.Range);
}
