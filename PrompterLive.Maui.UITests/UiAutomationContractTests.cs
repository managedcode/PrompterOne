namespace PrompterLive.Maui.UITests;

public sealed class UiAutomationContractTests
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));

    [Fact]
    public void SharedPages_ExposeStableSelectorsForCoreFlows()
    {
        var library = File.ReadAllText(Path.Combine(RepoRoot, "PrompterLive.Shared", "Pages", "LibraryPage.razor"));
        var editor = File.ReadAllText(Path.Combine(RepoRoot, "PrompterLive.Shared", "Pages", "EditorPage.razor"));
        var learn = File.ReadAllText(Path.Combine(RepoRoot, "PrompterLive.Shared", "Pages", "LearnPage.razor"));
        var teleprompter = File.ReadAllText(Path.Combine(RepoRoot, "PrompterLive.Shared", "Pages", "TeleprompterPage.razor"));
        var settings = File.ReadAllText(Path.Combine(RepoRoot, "PrompterLive.Shared", "Pages", "SettingsPage.razor"));

        Assert.Contains("data-testid=\"library-page\"", library, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"editor-page\"", editor, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"learn-page\"", learn, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"teleprompter-page\"", teleprompter, StringComparison.Ordinal);
        Assert.Contains("data-testid=\"settings-page\"", settings, StringComparison.Ordinal);
    }

    [Fact]
    public void MauiWebViewHost_LoadsTheSharedAssetsNeededByUiSmokeTests()
    {
        var indexHtml = File.ReadAllText(Path.Combine(RepoRoot, "PrompterLive.Maui", "wwwroot", "index.html"));

        Assert.Contains("_content/PrompterLive.Shared/app.css", indexHtml, StringComparison.Ordinal);
        Assert.Contains("_content/PrompterLive.Shared/prompterlive.js", indexHtml, StringComparison.Ordinal);
        Assert.Contains("blazor.webview.js", indexHtml, StringComparison.Ordinal);
    }
}
