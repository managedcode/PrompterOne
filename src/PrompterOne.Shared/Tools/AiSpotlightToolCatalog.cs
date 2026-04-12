using PrompterOne.Core.AI.Models;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightToolCatalog
{
    public static IReadOnlyList<AiSpotlightTool> BuildSpotlightSuggestions(ScriptArticleContext context) =>
        BuildAll(context);

    public static IReadOnlyList<AiSpotlightTool> BuildAgentTools(ScriptArticleContext context) =>
        BuildAll(context);

    private static IReadOnlyList<AiSpotlightTool> BuildAll(ScriptArticleContext context)
    {
        var tools = new List<AiSpotlightTool>(160);
        AiSpotlightAgentToolCatalog.AddTo(tools, context);
        AiSpotlightNavigationToolCatalog.AddTo(tools, context.Editor?.DocumentId);
        AiSpotlightLibraryToolCatalog.AddTo(tools);
        AiSpotlightEditorToolCatalog.AddTo(tools, context);
        AiSpotlightGraphToolCatalog.AddTo(tools, context);
        AiSpotlightLearnToolCatalog.AddTo(tools);
        AiSpotlightTeleprompterToolCatalog.AddTo(tools);
        AiSpotlightSettingsToolCatalog.AddTo(tools);
        AiSpotlightMediaToolCatalog.AddTo(tools);
        AiSpotlightStreamingToolCatalog.AddTo(tools);
        AiSpotlightHotkeyToolCatalog.AddTo(tools);
        return tools;
    }
}
