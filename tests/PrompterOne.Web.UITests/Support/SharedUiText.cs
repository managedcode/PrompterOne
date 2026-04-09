using System.Globalization;
using System.Resources;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Web.UITests;

internal static class SharedUiText
{
    private static readonly ResourceManager ResourceManager = new(typeof(SharedResource));

    public static string Text(UiTextKey key) =>
        ResourceManager.GetString(key.ToString(), CultureInfo.InvariantCulture)
        ?? throw new InvalidOperationException($"Missing shared UI text resource for key '{key}'.");
}
