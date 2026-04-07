using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Services;

internal static class BrowserThemeInteropMethodNames
{
    public const string ApplySettingsTheme = AppThemeRuntime.Interop.GlobalName + "." + AppThemeRuntime.Interop.ApplySettingsThemeMethod;
}
