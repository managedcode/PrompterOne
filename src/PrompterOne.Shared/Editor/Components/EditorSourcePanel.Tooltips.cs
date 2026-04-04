namespace PrompterOne.Shared.Components.Editor;

public partial class EditorSourcePanel
{
    private static string GetTooltipCssClass(EditorToolbarTooltipPlacement placement) =>
        placement switch
        {
            EditorToolbarTooltipPlacement.Toolbar => "ed-toolbar-tooltip ed-toolbar-tooltip--toolbar",
            EditorToolbarTooltipPlacement.ToolbarMenu => "ed-toolbar-tooltip ed-toolbar-tooltip--menu",
            EditorToolbarTooltipPlacement.FloatingToolbar => "ed-toolbar-tooltip ed-toolbar-tooltip--floating",
            EditorToolbarTooltipPlacement.FloatingMenu => "ed-toolbar-tooltip ed-toolbar-tooltip--floating-menu",
            _ => "ed-toolbar-tooltip"
        };

    private static string GetTooltipPlacementValue(EditorToolbarTooltipPlacement placement) =>
        placement switch
        {
            EditorToolbarTooltipPlacement.Toolbar => "toolbar",
            EditorToolbarTooltipPlacement.ToolbarMenu => "menu",
            EditorToolbarTooltipPlacement.FloatingToolbar => "floating",
            EditorToolbarTooltipPlacement.FloatingMenu => "floating-menu",
            _ => string.Empty
        };
}

internal enum EditorToolbarTooltipPlacement
{
    Toolbar,
    ToolbarMenu,
    FloatingToolbar,
    FloatingMenu
}
