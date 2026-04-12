using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Services;

internal static class AiSpotlightSuggestionProvider
{
    public static IReadOnlyList<AiSpotlightSuggestion> Build(ScriptArticleContext context)
    {
        var suggestions = new List<AiSpotlightSuggestion>(4)
        {
            BuildPrimaryCommand(context),
            BuildNavigation(context)
        };

        if (context.Screen == AppShellScreen.Editor.ToString())
        {
            suggestions.Add(new(
                AiSpotlightSuggestionKind.Graph,
                UiTextKey.AiSpotlightSuggestionInspectGraph,
                UiTextKey.AiSpotlightSuggestionInspectGraphDetail,
                "inspect the script graph and explain the important links"));
        }

        suggestions.Add(new(
            AiSpotlightSuggestionKind.Navigation,
            UiTextKey.AiSpotlightSuggestionOpenSettings,
            UiTextKey.AiSpotlightSuggestionOpenSettingsDetail,
            "open settings",
            AppRoutes.Settings));

        return suggestions;
    }

    private static AiSpotlightSuggestion BuildPrimaryCommand(ScriptArticleContext context) =>
        context.Editor?.SelectedRange is not null
            ? new(
                AiSpotlightSuggestionKind.Command,
                UiTextKey.AiSpotlightSuggestionRewriteSelection,
                UiTextKey.AiSpotlightSuggestionRewriteSelectionDetail,
                "rewrite the selected editor lines")
            : new(
                AiSpotlightSuggestionKind.Command,
                UiTextKey.AiSpotlightSuggestionAskContext,
                UiTextKey.AiSpotlightSuggestionAskContextDetail,
                "answer using the current screen context");

    private static AiSpotlightSuggestion BuildNavigation(ScriptArticleContext context) =>
        context.Screen == AppShellScreen.Library.ToString()
            ? new(
                AiSpotlightSuggestionKind.Navigation,
                UiTextKey.AiSpotlightSuggestionOpenEditor,
                UiTextKey.AiSpotlightSuggestionOpenEditorDetail,
                "open editor",
                AppRoutes.Editor)
            : new(
                AiSpotlightSuggestionKind.Navigation,
                UiTextKey.AiSpotlightSuggestionOpenLibrary,
                UiTextKey.AiSpotlightSuggestionOpenLibraryDetail,
                "open library",
                AppRoutes.Library);
}
