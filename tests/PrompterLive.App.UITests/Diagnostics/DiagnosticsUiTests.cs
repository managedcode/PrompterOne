using PrompterLive.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterLive.App.UITests;

public sealed class DiagnosticsUiTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task LibraryScreen_ShowsDiagnosticsBannerWhenFolderCreateFails()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.AddInitScriptAsync(
                $$"""
                (() => {
                    const storageKey = {{System.Text.Json.JsonSerializer.Serialize(BrowserTestConstants.Diagnostics.FolderStorageKey)}};
                    const detail = {{System.Text.Json.JsonSerializer.Serialize(BrowserTestConstants.Diagnostics.ForcedFailureDetail)}};
                    const toggleGlobal = {{System.Text.Json.JsonSerializer.Serialize(BrowserTestConstants.Diagnostics.FolderCreateFailureToggleGlobal)}};
                    const originalSetItem = Storage.prototype.setItem;

                    Storage.prototype.setItem = function (key, value) {
                        if (window[toggleGlobal] === true && key === storageKey) {
                            throw new Error(detail);
                        }

                        return originalSetItem.call(this, key, value);
                    };
                })();
                """);

            await page.GotoAsync(BrowserTestConstants.Routes.Library);
            await Expect(page.GetByTestId(UiTestIds.Library.Page)).ToBeVisibleAsync();

            await page.EvaluateAsync(
                $$"""
                ({ toggleGlobal }) => {
                    window[toggleGlobal] = true;
                }
                """,
                new
                {
                    toggleGlobal = BrowserTestConstants.Diagnostics.FolderCreateFailureToggleGlobal
                });

            await page.GetByTestId(UiTestIds.Library.FolderCreateStart).ClickAsync();
            await Expect(page.GetByTestId(UiTestIds.Library.NewFolderOverlay)).ToBeVisibleAsync();
            await page.GetByTestId(UiTestIds.Library.NewFolderName).FillAsync("Diagnostics Failure Folder");
            await page.GetByTestId(UiTestIds.Library.NewFolderSubmit).ClickAsync();

            await Expect(page.GetByTestId(UiTestIds.Diagnostics.Banner)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.Diagnostics.Banner))
                .ToContainTextAsync(BrowserTestConstants.Diagnostics.CreateFolderFailure);
            await Expect(page.GetByTestId(UiTestIds.Diagnostics.Banner))
                .ToContainTextAsync(BrowserTestConstants.Diagnostics.ForcedFailureDetail);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task AppShell_ShowsConnectivityOverlayForOfflineAndOnlineStates()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.Library);

            await page.Context.SetOfflineAsync(true);
            await Expect(page.GetByTestId(UiTestIds.Diagnostics.Connectivity)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.Diagnostics.Connectivity))
                .ToContainTextAsync(BrowserTestConstants.Diagnostics.ConnectivityOfflineTitle);

            await page.Context.SetOfflineAsync(false);
            await Expect(page.GetByTestId(UiTestIds.Diagnostics.Connectivity))
                .ToContainTextAsync(BrowserTestConstants.Diagnostics.ConnectivityOnlineTitle);

            await page.GetByTestId(UiTestIds.Diagnostics.ConnectivityDismiss).ClickAsync();
            await Expect(page.GetByTestId(UiTestIds.Diagnostics.Connectivity)).ToBeHiddenAsync();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
