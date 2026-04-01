using Microsoft.AspNetCore.Components;
using PrompterOne.Shared.Services;
using PrompterOne.Shared.Settings.Models;

namespace PrompterOne.Shared.Pages;

public partial class SettingsPage : IDisposable
{
    [Inject] private IBrowserSettingsChangeNotifier BrowserSettingsChangeNotifier { get; set; } = null!;

    private void InitializeCrossTabSync()
    {
        BrowserSettingsChangeNotifier.Changed += HandleBrowserSettingsChangedAsync;
    }

    public void Dispose()
    {
        BrowserSettingsChangeNotifier.Changed -= HandleBrowserSettingsChangedAsync;
    }

    private async Task HandleBrowserSettingsChangedAsync(BrowserSettingChangeNotification notification)
    {
        if (_loadState ||
            !notification.IsRemote ||
            !string.Equals(notification.Key, SettingsPagePreferences.StorageKey, StringComparison.Ordinal))
        {
            return;
        }

        await ReloadPreferencesAsync();
    }
}
