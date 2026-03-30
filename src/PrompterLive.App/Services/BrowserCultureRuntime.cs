using System.Globalization;
using Microsoft.JSInterop;
using PrompterLive.Core.Localization;

namespace PrompterLive.App.Services;

internal static class BrowserCultureRuntime
{
    private const string GetPreferredCultureMethodName = "PrompterLive.localization.getPreferredCulture";

    public static async Task ApplyPreferredCultureAsync(IJSRuntime jsRuntime)
    {
        var preferredCulture = await jsRuntime.InvokeAsync<string?>(GetPreferredCultureMethodName);
        var culture = CultureInfo.GetCultureInfo(AppCultureCatalog.ResolveSupportedCulture(preferredCulture));
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
