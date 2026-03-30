using PrompterLive.Shared.Contracts;

namespace PrompterLive.Shared.Services;

public sealed record AppShellState(
    AppShellScreen Screen,
    string Title,
    string Subtitle,
    string WpmLabel,
    string BreadcrumbLabel,
    string SearchText,
    string ScriptId)
{
    public static AppShellState Default { get; } = new(
        Screen: AppShellScreen.Library,
        Title: string.Empty,
        Subtitle: string.Empty,
        WpmLabel: string.Empty,
        BreadcrumbLabel: string.Empty,
        SearchText: string.Empty,
        ScriptId: string.Empty);
}
