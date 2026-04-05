using PrompterOne.Core.Models.Editor;

namespace PrompterOne.Shared.Components.Editor;

internal static class EditorFloatingToolbarActionFactory
{
    public static EditorToolbarActionDescriptor Ai(
        string key,
        string contentHtml,
        string tooltip,
        string testId,
        EditorAiAssistAction aiAction) =>
        new(key, EditorToolbarActionType.Ai, "efb-btn efb-ai", contentHtml, tooltip, testId, AiAction: aiAction, PreventMouseDown: true);

    public static EditorToolbarActionDescriptor ClearColor(string key, string tooltip, string testId) =>
        new(
            key,
            EditorToolbarActionType.Command,
            "efb-menu-item",
            "<span style=\"color:var(--text-4);font-weight:700;width:44px;text-align:right\">RESET</span> Remove cues",
            tooltip,
            testId,
            Command: new EditorCommandRequest(EditorCommandKind.ClearColor, string.Empty),
            PreventMouseDown: true);

    public static EditorToolbarActionDescriptor Insert(
        string key,
        string contentHtml,
        string tooltip,
        string testId,
        string token,
        string cssClass = "efb-btn") =>
        new(
            key,
            EditorToolbarActionType.Command,
            cssClass,
            contentHtml,
            tooltip,
            testId,
            Command: new EditorCommandRequest(EditorCommandKind.Insert, token),
            PreventMouseDown: true);

    public static EditorToolbarActionDescriptor Toggle(string menuId, string contentHtml, string tooltip, string testId) =>
        new(menuId, EditorToolbarActionType.ToggleMenu, "efb-btn", contentHtml, tooltip, testId, MenuId: menuId, PreventMouseDown: true);

    public static EditorToolbarActionDescriptor Wrap(
        string key,
        string contentHtml,
        string tooltip,
        string testId,
        string openingToken,
        string closingToken,
        string placeholder = "text",
        string cssClass = "efb-btn") =>
        new(
            key,
            EditorToolbarActionType.Command,
            cssClass,
            contentHtml,
            tooltip,
            testId,
            Command: new EditorCommandRequest(EditorCommandKind.Wrap, openingToken, closingToken, placeholder),
            PreventMouseDown: true);
}
