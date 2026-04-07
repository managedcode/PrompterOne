using PrompterOne.Shared.Services;
using PrompterOne.Shared.Settings.Models;

namespace PrompterOne.Shared.Contracts;

public static class AppThemeRuntime
{
    public static class Runtime
    {
        public const string ContractProperty = "theme";
        public const string GlobalName = AppRuntimeTelemetry.Harness.RuntimeGlobalName;
    }

    public static class Interop
    {
        public const string ApplySettingsThemeMethod = "applySettingsTheme";
        public const string GlobalName = "prompterOneTheme";
    }

    public static class RootAttributes
    {
        public const string Density = "data-density";
        public const string Theme = "data-theme";
        public const string ThemeSource = "data-theme-source";
    }

    public static class RootClasses
    {
        public const string ThemeDark = "theme-dark";
        public const string ThemeLight = "theme-light";
    }

    public static class Storage
    {
        public const string SettingsPageStorageKey = BrowserStorageKeys.SettingsPrefix + SettingsPagePreferences.StorageKey;
    }
}
