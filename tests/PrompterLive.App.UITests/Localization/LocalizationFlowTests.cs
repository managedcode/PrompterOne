using PrompterLive.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterLive.App.UITests;

public sealed class LocalizationFlowTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task LibraryScreen_UsesStoredFrenchCulture_ForLocalizedChrome()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.Library);
            await page.EvaluateAsync(
                BrowserTestConstants.Localization.SetLocalStorageScript,
                new[] { BrowserTestConstants.Localization.CultureStorageKey, BrowserTestConstants.Localization.FrenchCultureName });
            await page.ReloadAsync();

            await Expect(page.GetByTestId(UiTestIds.Library.SortLabel))
                .ToHaveTextAsync(BrowserTestConstants.Localization.FrenchSortByLabel);
            await Expect(page.GetByTestId(UiTestIds.Library.SectionFoldersTitle))
                .ToHaveTextAsync(BrowserTestConstants.Localization.FrenchFoldersLabel);

            await page.GetByTestId(UiTestIds.Library.FolderCreateStart).ClickAsync();
            await Expect(page.GetByTestId(UiTestIds.Library.NewFolderTitle))
                .ToHaveTextAsync(BrowserTestConstants.Localization.FrenchCreateFolderTitle);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
