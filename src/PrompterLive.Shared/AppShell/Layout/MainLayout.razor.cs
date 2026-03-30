using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Localization;
using PrompterLive.Shared.Services;

namespace PrompterLive.Shared.Layout;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    private const string AttachNavigatorLogMessage = "Attached SPA navigator bridge.";
    private const string ClientNavigationHomeLogMessage = "Client navigation requested for library.";
    private const string ClientNavigationEditorLogMessage = "Client navigation requested for editor.";
    private const string RouteChangedLogTemplate = "Route changed to {Location}.";

    private DotNetObjectReference<MainLayout>? _navigationBridge;

    [Inject] private AppBootstrapper Bootstrapper { get; set; } = null!;
    [Inject] private AppShellService Shell { get; set; } = null!;
    [Inject] private ILogger<MainLayout> Logger { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private AppShellState ShellState => Shell.State;

    private bool IsLibraryScreen => ShellState.Screen == AppShellScreen.Library;

    private bool ShowBackButton => ShellState.Screen != AppShellScreen.Library;

    private bool ShowLibraryActions => ShellState.Screen == AppShellScreen.Library;

    private bool ShowLearnAction => ShellState.Screen == AppShellScreen.Editor;

    private bool ShowGoLiveAction => ShellState.Screen != AppShellScreen.GoLive;

    private bool ShowLearnWpmBadge => ShellState.Screen == AppShellScreen.Learn;

    private bool ShowReadAction => ShellState.Screen is AppShellScreen.Editor or AppShellScreen.GoLive;

    private bool ShowHeaderSubtitle => !string.IsNullOrWhiteSpace(HeaderSubtitle);

    private string HeaderSubtitle => ShellState.Screen switch
    {
        AppShellScreen.Teleprompter => ShellState.Subtitle,
        AppShellScreen.GoLive => ShellState.Title,
        AppShellScreen.Learn => ShellState.Subtitle,
        _ => string.Empty
    };

    private string HeaderTitle => ShellState.Screen switch
    {
        AppShellScreen.GoLive => UiTextCatalog.Get(UiTextKey.HeaderGoLive),
        AppShellScreen.Settings => UiTextCatalog.Get(UiTextKey.HeaderSettings),
        _ => ShellState.Title
    };

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += HandleLocationChanged;
        Shell.StateChanged += HandleShellStateChanged;
        SyncShellStateWithCurrentRoute(Navigation.Uri);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await Bootstrapper.EnsureReadyAsync();
        _navigationBridge = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync(AppJsInterop.AttachNavigatorMethod, _navigationBridge);
        Logger.LogInformation(AttachNavigatorLogMessage);
    }

    [JSInvokable]
    public void NavigateHomeClient()
    {
        Logger.LogInformation(ClientNavigationHomeLogMessage);
        Navigation.NavigateTo(AppRoutes.Library);
    }

    [JSInvokable]
    public void NavigateEditorClient()
    {
        Logger.LogInformation(ClientNavigationEditorLogMessage);
        Navigation.NavigateTo(Shell.GetEditorRoute());
    }

    private void HandleShellStateChanged() => InvokeAsync(StateHasChanged);

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        Logger.LogInformation(RouteChangedLogTemplate, e.Location);
        SyncShellStateWithCurrentRoute(e.Location);
        StateHasChanged();
    }

    private void SyncShellStateWithCurrentRoute(string uri)
    {
        var currentScriptId = ResolveQueryValue(uri, AppRoutes.ScriptIdQueryKey);
        var currentPath = new Uri(uri).AbsolutePath.TrimEnd('/');

        switch (currentPath)
        {
            case "":
            case AppRoutes.Root:
            case AppRoutes.Library:
                Shell.ShowLibrary(string.IsNullOrWhiteSpace(ShellState.BreadcrumbLabel)
                    ? UiTextCatalog.Get(UiTextKey.LibraryAllScripts)
                    : ShellState.BreadcrumbLabel);
                break;
            case AppRoutes.Editor:
                Shell.ShowEditor(ShellState.Title, currentScriptId);
                break;
            case AppRoutes.Learn:
                Shell.ShowLearn(ShellState.Title, ShellState.Subtitle, ShellState.WpmLabel, currentScriptId);
                break;
            case AppRoutes.Teleprompter:
                Shell.ShowTeleprompter(ShellState.Title, ShellState.Subtitle, currentScriptId);
                break;
            case AppRoutes.GoLive:
                Shell.ShowGoLive(ShellState.Title, ShellState.Subtitle, currentScriptId);
                break;
            case AppRoutes.Settings:
                Shell.ShowSettings();
                break;
            default:
                Shell.ShowLibrary(UiTextCatalog.Get(UiTextKey.LibraryAllScripts));
                break;
        }
    }

    private static string ResolveQueryValue(string uri, string key)
    {
        var parsedUri = new Uri(uri);
        var query = parsedUri.Query;
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        var trimmedQuery = query.TrimStart('?');
        var pairs = trimmedQuery.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 0 || !string.Equals(parts[0], key, StringComparison.Ordinal))
            {
                continue;
            }

            return parts.Length > 1
                ? Uri.UnescapeDataString(parts[1])
                : string.Empty;
        }

        return string.Empty;
    }

    private Task HandleHomeClickAsync()
    {
        Navigation.NavigateTo(AppRoutes.Library);
        return Task.CompletedTask;
    }

    private Task HandleBackClickAsync()
    {
        Navigation.NavigateTo(AppRoutes.Library);
        return Task.CompletedTask;
    }

    private Task HandleCreateScriptClickAsync()
    {
        Navigation.NavigateTo(AppRoutes.Editor);
        return Task.CompletedTask;
    }

    private Task HandleOpenLearnClickAsync()
    {
        Navigation.NavigateTo(Shell.GetLearnRoute());
        return Task.CompletedTask;
    }

    private Task HandleOpenGoLiveClickAsync()
    {
        Navigation.NavigateTo(Shell.GetGoLiveRoute());
        return Task.CompletedTask;
    }

    private Task HandleOpenReadClickAsync()
    {
        Navigation.NavigateTo(Shell.GetTeleprompterRoute());
        return Task.CompletedTask;
    }

    private Task HandleLibrarySearchInputAsync(ChangeEventArgs args)
    {
        Shell.UpdateLibrarySearch(args.Value?.ToString() ?? string.Empty);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= HandleLocationChanged;
        Shell.StateChanged -= HandleShellStateChanged;
        _navigationBridge?.Dispose();
    }
}
