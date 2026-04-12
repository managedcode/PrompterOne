using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightEditorToolCatalog
{
    public static void AddTo(List<AiSpotlightTool> tools, ScriptArticleContext context)
    {
        AddLibraryDocumentTools(tools);

        if (context.Editor is null)
        {
            return;
        }

        AddEditorReadTools(tools);
        AddEditorMutationTools(tools, context);
    }

    private static void AddLibraryDocumentTools(List<AiSpotlightTool> tools)
    {
        AddMutation(tools, AiSpotlightToolNames.ScriptImport, UiTextKey.CommonImport, UiTextKey.ImportScriptMessage, AiSpotlightToolText.ScriptImport);
        AddReadOnly(tools, AiSpotlightToolNames.ScriptExport, UiTextKey.CommonExport, UiTextKey.EditorSaveFileMessage, AiSpotlightToolText.ScriptExport);
        AddMutation(tools, AiSpotlightToolNames.ScriptDuplicate, UiTextKey.CommonDuplicate, UiTextKey.AiSpotlightSuggestionOpenEditorDetail, AiSpotlightToolText.ScriptDuplicate);
        AddSensitive(tools, AiSpotlightToolNames.ScriptDelete, UiTextKey.CommonDelete, UiTextKey.TooltipDeleteScript, AiSpotlightToolText.ScriptDelete, destructive: true);
    }

    private static void AddEditorReadTools(List<AiSpotlightTool> tools)
    {
        AddReadOnly(tools, AiSpotlightToolNames.EditorSelectionRead, UiTextKey.CommonText, UiTextKey.AiSpotlightSuggestionRewriteSelectionDetail, AiSpotlightToolText.EditorSelectionRead);
        AddReadOnly(tools, AiSpotlightToolNames.ScriptRead, UiTextKey.CommonText, UiTextKey.EditorSaveFileMessage, AiSpotlightToolText.ScriptRead);
        AddReadOnly(tools, AiSpotlightToolNames.ScriptMetadataRead, UiTextKey.EditorMetadataSection, UiTextKey.EditorMetadataStatsSection, AiSpotlightToolText.ScriptMetadataRead);
        AddReadOnly(tools, AiSpotlightToolNames.EditorCursorContextRead, UiTextKey.EditorStructureActiveBlock, UiTextKey.EditorStructureSection, AiSpotlightToolText.EditorCursorContextRead);
        AddReadOnly(tools, AiSpotlightToolNames.EditorRangeRead, UiTextKey.CommonText, UiTextKey.EditorStatusLineShort, AiSpotlightToolText.EditorRangeRead, AiSpotlightToolParameterSets.Range);
        AddReadOnly(tools, AiSpotlightToolNames.EditorFind, UiTextKey.EditorFindPlaceholder, UiTextKey.EditorFindResultsFormat, AiSpotlightToolText.EditorFind, AiSpotlightToolParameterSets.Search);
        AddReadOnly(tools, AiSpotlightToolNames.ScriptOutlineRead, UiTextKey.EditorStructureSection, UiTextKey.EditorMetadataStatsSection, AiSpotlightToolText.ScriptOutlineRead);
        AddReadOnly(tools, AiSpotlightToolNames.ScriptHistoryOpen, UiTextKey.CommonHistory, UiTextKey.EditorMetadataHistoryHint, AiSpotlightToolText.ScriptHistoryOpen);
    }

    private static void AddEditorMutationTools(List<AiSpotlightTool> tools, ScriptArticleContext context)
    {
        AddMutation(tools, AiSpotlightToolNames.ScriptSave, UiTextKey.CommonSaveLocally, UiTextKey.EditorPersistDraftMessage, AiSpotlightToolText.ScriptSave);
        AddMutation(tools, AiSpotlightToolNames.ScriptSplit, UiTextKey.EditorMetadataSplitSection, UiTextKey.EditorMetadataSplitHint, AiSpotlightToolText.ScriptSplit);
        AddMutation(tools, AiSpotlightToolNames.EditorInsert, UiTextKey.EditorToolbarSectionInsert, UiTextKey.EditorToolbarTooltipInsertHelpers, AiSpotlightToolText.EditorInsert, AiSpotlightToolParameterSets.Insertion);
        AddMutation(tools, AiSpotlightToolNames.EditorReplace, UiTextKey.AiSpotlightSuggestionRewriteSelection, UiTextKey.AiSpotlightSuggestionRewriteSelectionDetail, AiSpotlightToolText.EditorReplace, AiSpotlightToolParameterSets.Replacement);
        AddMutation(tools, AiSpotlightToolNames.EditorFindReplace, UiTextKey.EditorFindPlaceholder, UiTextKey.AiSpotlightSuggestionRewriteSelectionDetail, AiSpotlightToolText.EditorFindReplace, AiSpotlightToolParameterSets.FindReplace);
        AddMutation(tools, AiSpotlightToolNames.TpsCueInsert, UiTextKey.EditorToolbarSectionInsert, UiTextKey.EditorToolbarTooltipInsertHelpers, AiSpotlightToolText.TpsCueInsert, AiSpotlightToolParameterSets.TpsCue);
        AddMutation(tools, AiSpotlightToolNames.TpsSelectionWrap, UiTextKey.EditorToolbarGroupTpsEmotions, UiTextKey.EditorToolbarTooltipVoiceCues, AiSpotlightToolText.TpsSelectionWrap, AiSpotlightToolParameterSets.TpsCue);
        AddMutation(tools, AiSpotlightToolNames.EditorUndo, UiTextKey.EditorToolbarTooltipUndo, UiTextKey.SettingsHotkeyEditorUndoDescription, AiSpotlightToolText.EditorUndo);
        AddMutation(tools, AiSpotlightToolNames.EditorRedo, UiTextKey.EditorToolbarTooltipRedo, UiTextKey.SettingsHotkeyEditorRedoDescription, AiSpotlightToolText.EditorRedo);
        AddSensitive(tools, AiSpotlightToolNames.EditorRangeDelete, UiTextKey.CommonDelete, UiTextKey.EditorToolbarTooltipRemoveInlineTags, AiSpotlightToolText.EditorRangeDelete, AiSpotlightToolParameterSets.Range, destructive: true);
        AddSensitive(tools, AiSpotlightToolNames.ScriptRevisionRestore, UiTextKey.CommonRestore, UiTextKey.EditorMetadataHistoryHint, AiSpotlightToolText.ScriptRevisionRestore, destructive: false);

        if (context.Editor?.SelectedRange is not null)
        {
            AddMutation(tools, AiSpotlightToolNames.EditorSelectionRewrite, UiTextKey.AiSpotlightSuggestionRewriteSelection, UiTextKey.AiSpotlightSuggestionRewriteSelectionDetail, AiSpotlightToolText.EditorSelectionRewrite, AiSpotlightToolParameterSets.Replacement);
            AddMutation(tools, AiSpotlightToolNames.EditorSelectionToTps, UiTextKey.AiSpotlightSuggestionRewriteSelection, UiTextKey.AiSpotlightSuggestionRewriteSelectionDetail, AiSpotlightToolText.EditorSelectionToTps, AiSpotlightToolParameterSets.Replacement);
        }
    }

    private static void AddReadOnly(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        UiTextKey detail,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            label,
            detail,
            prompt,
            AiSpotlightToolScopes.Editor,
            parameters));

    private static void AddMutation(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        UiTextKey detail,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            label,
            detail,
            prompt,
            AiSpotlightToolScopes.Editor,
            parameters,
            readOnly: false,
            idempotent: false));

    private static void AddSensitive(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        UiTextKey detail,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null,
        bool destructive = false) =>
        tools.Add(AiSpotlightToolFactory.SensitiveMutationTool(
            name,
            label,
            detail,
            prompt,
            AiSpotlightToolScopes.Editor,
            parameters,
            destructive));
}
