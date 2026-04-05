using Microsoft.AspNetCore.Components;
using PrompterOne.Shared.Services;
using PrompterOne.Shared.Settings.Models;
using PrompterOne.Shared.Settings.Services;

namespace PrompterOne.Shared.Components.Editor;

public partial class EditorSourcePanel
{
    private bool _hasConfiguredAiProvider;
    private bool _needsInitialAiAvailabilityLoad = true;

    [Inject] private AiProviderSettingsStore AiProviderSettingsStore { get; set; } = null!;

    [Inject] private IBrowserSettingsChangeNotifier BrowserSettingsChangeNotifier { get; set; } = null!;

    protected override void OnInitialized()
    {
        BrowserSettingsChangeNotifier.Changed += HandleBrowserSettingsChangedAsync;
    }

    private async Task<bool> TryLoadInitialAiAvailabilityAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !_needsInitialAiAvailabilityLoad)
        {
            return false;
        }

        _needsInitialAiAvailabilityLoad = false;
        await RefreshAiAvailabilityAsync();
        StateHasChanged();
        return true;
    }

    private async Task HandleBrowserSettingsChangedAsync(BrowserSettingChangeNotification notification)
    {
        if (!string.Equals(notification.Key, AiProviderSettings.StorageKey, StringComparison.Ordinal))
        {
            return;
        }

        await RefreshAiAvailabilityAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task RefreshAiAvailabilityAsync()
    {
        var settings = await AiProviderSettingsStore.LoadAsync();
        _hasConfiguredAiProvider = settings.HasConfiguredProvider();
    }
}
