namespace PrompterOne.Shared.Components.Editor;

internal static partial class EditorTpsMenuCatalog
{
    private const string ToolbarMenuItemCssClass = "tb-emo-item tb-tip";
    private const string FloatingMenuItemCssClass = "efb-menu-item";
    private const string DefaultPlaceholder = "text";
    private const string UnwrapMetaText = "unwrap";

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildToolbarEmotionGroups() =>
        BuildGroups(CreateEmotionGroupDefinitions(), EditorTpsMenuSurface.Toolbar);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildToolbarInsertGroups() =>
        BuildGroups(CreateInsertGroupDefinitions(), EditorTpsMenuSurface.Toolbar);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildToolbarPauseGroups() =>
        BuildGroups(CreatePauseGroupDefinitions(), EditorTpsMenuSurface.Toolbar);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildToolbarSpeedGroups() =>
        BuildGroups(CreateSpeedGroupDefinitions(), EditorTpsMenuSurface.Toolbar);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildToolbarVoiceGroups() =>
        BuildGroups(CreateVoiceGroupDefinitions(), EditorTpsMenuSurface.Toolbar);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildFloatingEmotionGroups() =>
        BuildGroups(CreateEmotionGroupDefinitions(), EditorTpsMenuSurface.Floating);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildFloatingInsertGroups() =>
        BuildGroups(CreateInsertGroupDefinitions(), EditorTpsMenuSurface.Floating);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildFloatingPauseGroups() =>
        BuildGroups(CreatePauseGroupDefinitions(), EditorTpsMenuSurface.Floating);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildFloatingSpeedGroups() =>
        BuildGroups(CreateSpeedGroupDefinitions(), EditorTpsMenuSurface.Floating);

    public static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildFloatingVoiceGroups() =>
        BuildGroups(CreateVoiceGroupDefinitions(), EditorTpsMenuSurface.Floating);

    private static IReadOnlyList<EditorToolbarDropdownGroupDescriptor> BuildGroups(
        IReadOnlyList<EditorTpsMenuGroupDefinition> definitions,
        EditorTpsMenuSurface surface) =>
        definitions
            .Select(definition => new EditorToolbarDropdownGroupDescriptor(
                definition.Label,
                definition.Actions.Select(action => CreateAction(action, surface)).ToArray(),
                definition.HasSeparatorBefore))
            .ToArray();

    private static EditorToolbarActionDescriptor CreateAction(
        EditorTpsMenuActionDefinition definition,
        EditorTpsMenuSurface surface)
    {
        var key = surface == EditorTpsMenuSurface.Toolbar ? definition.ToolbarKey : definition.FloatingKey;
        var testId = surface == EditorTpsMenuSurface.Toolbar ? definition.ToolbarTestId : definition.FloatingTestId;
        return definition.Kind switch
        {
            EditorTpsMenuActionKind.Wrap => CreateWrapAction(
                surface,
                key,
                definition.Content,
                definition.Tooltip,
                testId,
                definition.PrimaryToken,
                definition.SecondaryToken ?? string.Empty,
                definition.Placeholder),
            EditorTpsMenuActionKind.Insert => CreateInsertAction(
                surface,
                key,
                definition.Content,
                definition.Tooltip,
                testId,
                definition.PrimaryToken),
            EditorTpsMenuActionKind.ClearColor => CreateClearAction(surface, key, definition.Tooltip, testId),
            _ => throw new InvalidOperationException($"Unsupported TPS menu action kind '{definition.Kind}'.")
        };
    }

    private static EditorToolbarActionDescriptor CreateWrapAction(
        EditorTpsMenuSurface surface,
        string key,
        EditorActionContentDescriptor content,
        string tooltip,
        string testId,
        string openingToken,
        string closingToken,
        string placeholder) =>
        surface == EditorTpsMenuSurface.Toolbar
            ? new EditorToolbarActionDescriptor(
                key,
                EditorToolbarActionType.Command,
                ToolbarMenuItemCssClass,
                content,
                tooltip,
                testId,
                Command: new EditorCommandRequest(EditorCommandKind.Wrap, openingToken, closingToken, placeholder),
                PreventMouseDown: true)
            : EditorFloatingToolbarActionFactory.Wrap(
                key,
                content,
                tooltip,
                testId,
                openingToken,
                closingToken,
                placeholder,
                FloatingMenuItemCssClass);

    private static EditorToolbarActionDescriptor CreateInsertAction(
        EditorTpsMenuSurface surface,
        string key,
        EditorActionContentDescriptor content,
        string tooltip,
        string testId,
        string token) =>
        surface == EditorTpsMenuSurface.Toolbar
            ? new EditorToolbarActionDescriptor(
                key,
                EditorToolbarActionType.Command,
                ToolbarMenuItemCssClass,
                content,
                tooltip,
                testId,
                Command: new EditorCommandRequest(EditorCommandKind.Insert, token),
                PreventMouseDown: true)
            : EditorFloatingToolbarActionFactory.Insert(
                key,
                content,
                tooltip,
                testId,
                token,
                FloatingMenuItemCssClass);

    private static EditorToolbarActionDescriptor CreateClearAction(
        EditorTpsMenuSurface surface,
        string key,
        string tooltip,
        string testId) =>
        surface == EditorTpsMenuSurface.Toolbar
            ? new EditorToolbarActionDescriptor(
                key,
                EditorToolbarActionType.Command,
                ToolbarMenuItemCssClass,
                ResetContent,
                tooltip,
                testId,
                Command: new EditorCommandRequest(EditorCommandKind.ClearColor, string.Empty),
                PreventMouseDown: true)
            : EditorFloatingToolbarActionFactory.ClearColor(key, tooltip, testId);

    private static EditorTpsMenuActionDefinition Clear(
        string toolbarKey,
        string floatingKey,
        string toolbarTestId,
        string floatingTestId,
        string tooltip) =>
        new(
            EditorTpsMenuActionKind.ClearColor,
            toolbarKey,
            floatingKey,
            toolbarTestId,
            floatingTestId,
            ResetContent,
            tooltip,
            string.Empty);

    private static EditorTpsMenuActionDefinition Insert(
        string toolbarKey,
        string floatingKey,
        string toolbarTestId,
        string floatingTestId,
        EditorActionContentDescriptor content,
        string tooltip,
        string token) =>
        new(
            EditorTpsMenuActionKind.Insert,
            toolbarKey,
            floatingKey,
            toolbarTestId,
            floatingTestId,
            content,
            tooltip,
            token);

    private static EditorTpsMenuActionDefinition Wrap(
        string toolbarKey,
        string floatingKey,
        string toolbarTestId,
        string floatingTestId,
        EditorActionContentDescriptor content,
        string tooltip,
        string openingToken,
        string closingToken,
        string placeholder = DefaultPlaceholder) =>
        new(
            EditorTpsMenuActionKind.Wrap,
            toolbarKey,
            floatingKey,
            toolbarTestId,
            floatingTestId,
            content,
            tooltip,
            openingToken,
            closingToken,
            placeholder);

    private static EditorActionContentDescriptor ResetContent =>
        EditorActionContents.Label("RESET", "Remove cues", UnwrapMetaText, EditorActionContentTone.Reset);
}

internal enum EditorTpsMenuActionKind
{
    Wrap,
    Insert,
    ClearColor
}

internal sealed record EditorTpsMenuActionDefinition(
    EditorTpsMenuActionKind Kind,
    string ToolbarKey,
    string FloatingKey,
    string ToolbarTestId,
    string FloatingTestId,
    EditorActionContentDescriptor Content,
    string Tooltip,
    string PrimaryToken,
    string? SecondaryToken = null,
    string Placeholder = "text");

internal sealed record EditorTpsMenuGroupDefinition(
    string Label,
    IReadOnlyList<EditorTpsMenuActionDefinition> Actions,
    bool HasSeparatorBefore = false);

internal enum EditorTpsMenuSurface
{
    Toolbar,
    Floating
}
