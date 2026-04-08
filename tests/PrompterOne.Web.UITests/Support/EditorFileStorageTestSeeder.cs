using System.Text.Json;
using Microsoft.Playwright;
using PrompterOne.Shared.Services;
using PrompterOne.Shared.Settings.Models;

namespace PrompterOne.Web.UITests;

internal static class EditorFileStorageTestSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string BrowserFileStorageSettingsKey =
        string.Concat(BrowserStorageKeys.SettingsPrefix, BrowserFileStorageSettings.StorageKey);

    internal static Task SeedAutoSaveDisabledAsync(IPage page)
    {
        var settingsJson = JsonSerializer.Serialize(
            BrowserFileStorageSettings.Default with { FileAutoSaveEnabled = false },
            JsonOptions);

        return page.EvaluateAsync(
            BrowserTestConstants.Localization.SetLocalStorageScript,
            new object[]
            {
                BrowserFileStorageSettingsKey,
                settingsJson
            });
    }
}
