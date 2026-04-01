using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class NavigationFlowTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task ShellHeader_OpensGoLive_FromLibraryAndSettings()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.Library);
            await Expect(page.GetByTestId(UiTestIds.Library.Page)).ToBeVisibleAsync();

            await page.GetByTestId(UiTestIds.Header.GoLive).ClickAsync();
            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(AppRoutes.GoLive));
            await Expect(page.GetByTestId(UiTestIds.GoLive.Page)).ToBeVisibleAsync();

            await page.GotoAsync(BrowserTestConstants.Routes.Settings);
            await Expect(page.GetByTestId(UiTestIds.Settings.Page)).ToBeVisibleAsync();

            await page.GetByTestId(UiTestIds.Header.GoLive).ClickAsync();
            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(AppRoutes.GoLive));
            await Expect(page.GetByTestId(UiTestIds.GoLive.Page)).ToBeVisibleAsync();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task ScreenNavigation_UsesSpaRoutingWithoutReloadingBrowserContext()
    {
        const string nonce = "spa-nav-stable";
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorQuantum);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await page.EvaluateAsync("value => window.__prompterSpaNonce = value", nonce);

            await page.GetByTestId(UiTestIds.Header.EditorLearn).ClickAsync();
            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(BrowserTestConstants.Routes.LearnQuantum));
            await Expect(page.GetByTestId(UiTestIds.Learn.Page)).ToBeVisibleAsync();
            Assert.Equal(nonce, await page.EvaluateAsync<string>("() => window.__prompterSpaNonce"));

            await page.GotoAsync(BrowserTestConstants.Routes.EditorQuantum);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page)).ToBeVisibleAsync();
            await page.EvaluateAsync("value => window.__prompterSpaNonce = value", nonce);

            await page.GetByTestId(UiTestIds.Header.EditorRead).ClickAsync();
            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(BrowserTestConstants.Routes.TeleprompterQuantum));
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.Page)).ToBeVisibleAsync();
            Assert.Equal(nonce, await page.EvaluateAsync<string>("() => window.__prompterSpaNonce"));

            await page.GetByTestId(UiTestIds.Teleprompter.Back).ClickAsync();
            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(BrowserTestConstants.Routes.EditorQuantum));
            await Expect(page.GetByTestId(UiTestIds.Editor.Page)).ToBeVisibleAsync();
            Assert.Equal(nonce, await page.EvaluateAsync<string>("() => window.__prompterSpaNonce"));
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
