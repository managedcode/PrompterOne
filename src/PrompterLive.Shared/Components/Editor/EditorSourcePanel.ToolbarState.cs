using PrompterLive.Core.Models.Editor;

namespace PrompterLive.Shared.Components.Editor;

public partial class EditorSourcePanel
{
    private bool _isAiPanelOpen;
    private string? _openMenuId;
    private bool _suppressNextSelectionPanelClose;

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
                ToggleAiPanel();
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
        _isAiPanelOpen = false;
        _openMenuId = null;
        _suppressNextSelectionPanelClose = false;
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

    private void ToggleAiPanel()
    {
        _isAiPanelOpen = !_isAiPanelOpen;
        _openMenuId = null;
        _suppressNextSelectionPanelClose = _isAiPanelOpen;
    }

    private void ToggleMenu(string menuId)
    {
        _isAiPanelOpen = false;
        _suppressNextSelectionPanelClose = false;
        _openMenuId = IsMenuOpen(menuId) ? null : menuId;
    }

    private bool TryConsumeSelectionCloseSuppression()
    {
        if (!_suppressNextSelectionPanelClose)
        {
            return false;
        }

        _suppressNextSelectionPanelClose = false;
        return true;
    }
}
