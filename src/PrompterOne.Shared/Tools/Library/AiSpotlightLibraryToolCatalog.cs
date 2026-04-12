using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightLibraryToolCatalog
{
    public static void AddTo(List<AiSpotlightTool> tools)
    {
        AddReadOnly(tools, AiSpotlightToolNames.LibraryScriptsList, UiTextKey.CommonScripts, AiSpotlightToolText.LibraryScriptsList);
        AddReadOnly(tools, AiSpotlightToolNames.LibraryScriptsSearch, UiTextKey.HeaderSearchPlaceholder, AiSpotlightToolText.LibraryScriptsSearch, AiSpotlightToolParameterSets.LibrarySearch);
        AddReadOnly(tools, AiSpotlightToolNames.LibraryScriptMetadataRead, UiTextKey.CommonStats, AiSpotlightToolText.LibraryScriptMetadataRead, AiSpotlightToolParameterSets.Script);
        AddReadOnly(tools, AiSpotlightToolNames.LibraryFoldersList, UiTextKey.LibraryFolders, AiSpotlightToolText.LibraryFoldersList);
        AddMutation(tools, AiSpotlightToolNames.LibraryFolderCreate, UiTextKey.LibraryNewFolder, AiSpotlightToolText.LibraryFolderCreate);
        AddMutation(tools, AiSpotlightToolNames.LibraryScriptRename, UiTextKey.CommonRename, AiSpotlightToolText.LibraryScriptRename, AiSpotlightToolParameterSets.Script);
        AddMutation(tools, AiSpotlightToolNames.LibraryScriptMove, UiTextKey.CommonMoveToEllipsis, AiSpotlightToolText.LibraryScriptMove, AiSpotlightToolParameterSets.Script);
        AddSensitive(tools, AiSpotlightToolNames.LibraryScriptDelete, UiTextKey.CommonDelete, AiSpotlightToolText.LibraryScriptDelete, AiSpotlightToolParameterSets.Script);
    }

    private static void AddReadOnly(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            label,
            UiTextKey.AiSpotlightSuggestionOpenLibraryDetail,
            prompt,
            AiSpotlightToolScopes.Library,
            parameters));

    private static void AddMutation(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            label,
            UiTextKey.AiSpotlightSuggestionOpenLibraryDetail,
            prompt,
            AiSpotlightToolScopes.Library,
            parameters,
            readOnly: false,
            idempotent: false));

    private static void AddSensitive(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter> parameters) =>
        tools.Add(AiSpotlightToolFactory.SensitiveMutationTool(
            name,
            label,
            UiTextKey.AiSpotlightSuggestionOpenLibraryDetail,
            prompt,
            AiSpotlightToolScopes.Library,
            parameters,
            destructive: true));
}
