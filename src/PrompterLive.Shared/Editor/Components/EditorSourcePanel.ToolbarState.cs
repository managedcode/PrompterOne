using PrompterLive.Core.Models.Editor;

namespace PrompterLive.Shared.Components.Editor;

public partial class EditorSourcePanel
{
    private string? _openMenuId;

    private static IReadOnlyList<EditorToolbarSectionDescriptor> ToolbarSections => EditorToolbarCatalog.Sections;

    private static IReadOnlyList<IReadOnlyList<EditorToolbarActionDescriptor>> FloatingActionGroups => EditorToolbarCatalog.FloatingActionGroups;

    private Task ExecuteAiActionAsync(EditorAiAssistAction action)
    {
        CloseToolbarPanels();
        return OnAiActionRequested.InvokeAsync(action);
    }

    private async Task ExecuteToolbarActionAsync(EditorToolbarActionDescriptor action)
    {
        switch (action.ActionType)
        {
            case EditorToolbarActionType.ToggleMenu:
                ToggleMenu(action.MenuId ?? string.Empty);
                break;
            case EditorToolbarActionType.History:
                CloseToolbarPanels();
                if (action.HistoryCommand is { } historyCommand)
                {
                    await RequestHistoryAsync(historyCommand);
                }

                break;
            case EditorToolbarActionType.Ai:
                CloseToolbarPanels();
                if (action.AiAction is { } aiAction)
                {
                    await ExecuteAiActionAsync(aiAction);
                }

                break;
            default:
                CloseToolbarPanels();
                if (action.Command is { } command)
                {
                    await OnCommandRequested.InvokeAsync(command);
                }

                break;
        }
    }

    private void CloseToolbarPanels()
    {
        _openMenuId = null;
    }

    private string GetToolbarSectionCss(EditorToolbarSectionDescriptor section)
    {
        if (!section.HasDropdown)
        {
            return "tb-section";
        }

        return IsMenuOpen(section.MenuId!)
            ? "tb-section tb-dropdown-wrap open"
            : "tb-section tb-dropdown-wrap";
    }

    private string? GetActionAriaExpanded(EditorToolbarActionDescriptor action) =>
        action.ActionType == EditorToolbarActionType.ToggleMenu && !string.IsNullOrWhiteSpace(action.MenuId)
            ? IsMenuOpen(action.MenuId) ? "true" : "false"
            : null;

    private static string GetDropdownListCss(EditorToolbarSectionDescriptor section) =>
        string.Equals(section.Key, "color", StringComparison.Ordinal)
            ? "tb-color-grid"
            : "tb-emo-list";

    private bool GetActionDisabled(EditorToolbarActionDescriptor action) =>
        action.HistoryCommand switch
        {
            EditorHistoryCommand.Undo => !CanUndo,
            EditorHistoryCommand.Redo => !CanRedo,
            _ => false
        };

    private bool IsMenuOpen(string menuId) =>
        string.Equals(_openMenuId, menuId, StringComparison.Ordinal);

    private void ToggleMenu(string menuId)
    {
        _openMenuId = IsMenuOpen(menuId) ? null : menuId;
    }
}
