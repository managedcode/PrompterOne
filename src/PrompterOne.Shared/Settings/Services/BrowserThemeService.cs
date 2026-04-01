using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using PrompterOne.Core.Abstractions;
using PrompterOne.Shared.Settings.Models;

namespace PrompterOne.Shared.Services;

public sealed class BrowserThemeService : IDisposable
{
    private const string ApplyThemeFailureMessage = "Failed to apply browser theme settings.";
    private const string InitializeThemeFailureMessage = "Failed to initialize browser theme settings.";
    private const string RemoteThemeSyncFailureMessage = "Failed to apply browser theme settings after a cross-tab update.";

    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<BrowserThemeService> _logger;
    private readonly IBrowserSettingsChangeNotifier _settingsChangeNotifier;
    private readonly IUserSettingsStore _settingsStore;

    private bool _initialized;

    public BrowserThemeService(
        IJSRuntime jsRuntime,
        IUserSettingsStore settingsStore,
        IBrowserSettingsChangeNotifier settingsChangeNotifier,
        ILogger<BrowserThemeService>? logger = null)
    {
        _jsRuntime = jsRuntime;
        _settingsStore = settingsStore;
        _settingsChangeNotifier = settingsChangeNotifier;
        _logger = logger ?? NullLogger<BrowserThemeService>.Instance;
        _settingsChangeNotifier.Changed += HandleSettingsChangedAsync;
    }

    public async Task ApplyAsync(SettingsPagePreferences preferences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        try
        {
            await _jsRuntime.InvokeVoidAsync(
                BrowserThemeInteropMethodNames.ApplySettingsTheme,
                cancellationToken,
                preferences.ColorScheme,
                preferences.AccentColor,
                preferences.UiDensity);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ApplyThemeFailureMessage);
            throw;
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            var preferences = await _settingsStore.LoadAsync<SettingsPagePreferences>(
                SettingsPagePreferences.StorageKey,
                cancellationToken)
                ?? SettingsPagePreferences.Default;

            await ApplyAsync(preferences, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, InitializeThemeFailureMessage);
            throw;
        }
    }

    public void Dispose()
    {
        _settingsChangeNotifier.Changed -= HandleSettingsChangedAsync;
    }

    private async Task HandleSettingsChangedAsync(BrowserSettingChangeNotification notification)
    {
        if (!_initialized ||
            !notification.IsRemote ||
            !string.Equals(notification.Key, SettingsPagePreferences.StorageKey, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            var preferences = await _settingsStore.LoadAsync<SettingsPagePreferences>(SettingsPagePreferences.StorageKey)
                ?? SettingsPagePreferences.Default;

            await ApplyAsync(preferences);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, RemoteThemeSyncFailureMessage);
        }
    }
}
