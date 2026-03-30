using PrompterLive.Shared.Contracts;

namespace PrompterLive.Shared.Services;

public sealed class AppShellService
{
    public event Action? StateChanged;
    public event Action<string>? LibrarySearchChanged;

    public AppShellState State { get; private set; } = AppShellState.Default;

    public void ShowLibrary(string breadcrumbLabel)
    {
        var nextSearchText = State.Screen == AppShellScreen.Library
            ? State.SearchText
            : string.Empty;

        SetState(new AppShellState(
            Screen: AppShellScreen.Library,
            Title: string.Empty,
            Subtitle: string.Empty,
            WpmLabel: string.Empty,
            BreadcrumbLabel: breadcrumbLabel,
            SearchText: nextSearchText,
            ScriptId: string.Empty));
    }

    public void ShowEditor(string title, string? scriptId) =>
        SetScriptScopedState(AppShellScreen.Editor, title, string.Empty, string.Empty, scriptId);

    public void ShowLearn(string title, string subtitle, string wpmLabel, string? scriptId) =>
        SetScriptScopedState(AppShellScreen.Learn, title, subtitle, wpmLabel, scriptId);

    public void ShowTeleprompter(string title, string subtitle, string? scriptId) =>
        SetScriptScopedState(AppShellScreen.Teleprompter, title, subtitle, string.Empty, scriptId);

    public void ShowGoLive(string title, string subtitle, string? scriptId) =>
        SetScriptScopedState(AppShellScreen.GoLive, title, subtitle, string.Empty, scriptId);

    public void ShowSettings() =>
        SetState(new AppShellState(
            Screen: AppShellScreen.Settings,
            Title: string.Empty,
            Subtitle: string.Empty,
            WpmLabel: string.Empty,
            BreadcrumbLabel: string.Empty,
            SearchText: string.Empty,
            ScriptId: string.Empty));

    public void UpdateLibrarySearch(string searchText)
    {
        var normalizedSearchText = searchText ?? string.Empty;
        if (string.Equals(State.SearchText, normalizedSearchText, StringComparison.Ordinal))
        {
            return;
        }

        SetState(State with { SearchText = normalizedSearchText });
        LibrarySearchChanged?.Invoke(normalizedSearchText);
    }

    public string GetEditorRoute() => BuildScriptScopedRoute(AppShellScreen.Editor);

    public string GetLearnRoute() => BuildScriptScopedRoute(AppShellScreen.Learn);

    public string GetTeleprompterRoute() => BuildScriptScopedRoute(AppShellScreen.Teleprompter);

    public string GetGoLiveRoute() => BuildScriptScopedRoute(AppShellScreen.GoLive);

    private void SetScriptScopedState(
        AppShellScreen screen,
        string title,
        string subtitle,
        string wpmLabel,
        string? scriptId)
    {
        SetState(new AppShellState(
            Screen: screen,
            Title: title,
            Subtitle: subtitle,
            WpmLabel: wpmLabel,
            BreadcrumbLabel: string.Empty,
            SearchText: string.Empty,
            ScriptId: scriptId ?? string.Empty));
    }

    private void SetState(AppShellState nextState)
    {
        if (EqualityComparer<AppShellState>.Default.Equals(State, nextState))
        {
            return;
        }

        State = nextState;
        StateChanged?.Invoke();
    }

    private string BuildScriptScopedRoute(AppShellScreen screen)
    {
        var scriptId = State.ScriptId;

        return screen switch
        {
            AppShellScreen.Editor => AppRoutes.EditorWithId(scriptId),
            AppShellScreen.Learn => AppRoutes.LearnWithId(scriptId),
            AppShellScreen.Teleprompter => AppRoutes.TeleprompterWithId(scriptId),
            AppShellScreen.GoLive => AppRoutes.GoLiveWithId(scriptId),
            _ => AppRoutes.Library
        };
    }
}
