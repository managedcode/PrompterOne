using Microsoft.JSInterop;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Settings.Models;

namespace PrompterOne.Shared.Services;

public sealed class ThemeRuntimeContractService(IJSRuntime jsRuntime) : IDisposable, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private bool _initializationAttempted;
    private Task<IJSObjectReference?>? _moduleTask;

    public async Task InitializeAsync()
    {
        if (_initializationAttempted)
        {
            return;
        }

        _initializationAttempted = true;

        var module = await GetModuleAsync();
        if (module is null)
        {
            return;
        }

        await module.InvokeVoidAsync(
            ThemeRuntimeContractInteropMethodNames.Configure,
            new
            {
                contractProperty = AppThemeRuntime.Runtime.ContractProperty,
                darkTheme = SettingsAppearanceValues.DarkColorScheme,
                defaultAccent = SettingsAppearanceValues.DefaultAccentColor,
                defaultDensity = SettingsAppearanceValues.DefaultDensity,
                lightTheme = SettingsAppearanceValues.LightColorScheme,
                runtimeGlobalName = AppThemeRuntime.Runtime.GlobalName,
                settingsPageStorageKey = AppThemeRuntime.Storage.SettingsPageStorageKey,
                systemTheme = SettingsAppearanceValues.SystemColorScheme,
                themeDarkClass = AppThemeRuntime.RootClasses.ThemeDark,
                themeGlobalName = AppThemeRuntime.Interop.GlobalName,
                themeLightClass = AppThemeRuntime.RootClasses.ThemeLight,
                themeRootAttribute = AppThemeRuntime.RootAttributes.Theme,
                themeSourceAttribute = AppThemeRuntime.RootAttributes.ThemeSource,
                densityRootAttribute = AppThemeRuntime.RootAttributes.Density
            });
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask is null)
        {
            return;
        }

        var module = await _moduleTask;
        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }

    public void Dispose()
    {
    }

    private Task<IJSObjectReference?> GetModuleAsync() =>
        _moduleTask ??= ImportModuleAsync();

    private async Task<IJSObjectReference?> ImportModuleAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<IJSObjectReference>(
                ThemeRuntimeContractInteropMethodNames.JSImportMethodName,
                ThemeRuntimeContractInteropMethodNames.ModulePath);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (JSException)
        {
            return null;
        }
    }
}
