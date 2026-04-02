using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class LibraryResponsiveLayoutTests(StandaloneAppFixture fixture) : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    public static TheoryData<string, int, int> ResponsiveViewports =>
        BrowserTestConstants.LibraryResponsive.Viewports.Aggregate(
            new TheoryData<string, int, int>(),
            static (data, viewport) =>
            {
                data.Add(viewport.Name, viewport.Width, viewport.Height);
                return data;
            });

    [Theory]
    [MemberData(nameof(ResponsiveViewports))]
    public Task LibraryScreen_KeepsNavigationAndSettingsAccessibleAcrossResponsiveViewports(string viewportName, int viewportWidth, int viewportHeight) =>
        RunPageAsync(async page =>
        {
            await page.SetViewportSizeAsync(viewportWidth, viewportHeight);
            await page.GotoAsync(BrowserTestConstants.Routes.Library);

            await Expect(page.GetByTestId(UiTestIds.Library.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.Library.FolderAll)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.Library.SectionFoldersTitle)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.Library.FolderCreateStart)).ToBeVisibleAsync();

            var settings = page.GetByTestId(UiTestIds.Library.OpenSettings);
            await Expect(settings).ToBeVisibleAsync();
            await AssertFullyWithinViewportAsync(settings, viewportName, viewportWidth, viewportHeight);

            await UiScenarioArtifacts.CapturePageAsync(
                page,
                BuildScenarioName(viewportName),
                BrowserTestConstants.LibraryResponsive.InitialStep);

            await AssertMainHeaderControlAccessibleAsync(page, page.GetByTestId(UiTestIds.Header.LibrarySearch), viewportName, viewportWidth, viewportHeight);
            await AssertMainHeaderControlAccessibleAsync(page, page.GetByTestId(UiTestIds.Header.LibraryNewScript), viewportName, viewportWidth, viewportHeight);
            await AssertMainHeaderControlAccessibleAsync(page, page.GetByTestId(UiTestIds.Header.GoLive), viewportName, viewportWidth, viewportHeight);
        });

    private static string BuildScenarioName(string viewportName) =>
        string.Concat(BrowserTestConstants.LibraryResponsive.ScenarioName, "-", viewportName);

    private static async Task AssertMainHeaderControlAccessibleAsync(
        IPage page,
        ILocator locator,
        string viewportName,
        int viewportWidth,
        int viewportHeight)
    {
        await locator.ScrollIntoViewIfNeededAsync();
        await Expect(locator).ToBeVisibleAsync();
        await AssertFullyWithinViewportAsync(locator, viewportName, viewportWidth, viewportHeight);
    }

    private static async Task AssertFullyWithinViewportAsync(
        ILocator locator,
        string viewportName,
        int viewportWidth,
        int viewportHeight)
    {
        var box = await locator.BoundingBoxAsync();
        Assert.NotNull(box);

        var bounds = box!;
        var tolerance = BrowserTestConstants.LibraryResponsive.ViewportEdgeTolerancePx;

        Assert.InRange(bounds.X, -tolerance, viewportWidth);
        Assert.InRange(bounds.Y, -tolerance, viewportHeight);
        Assert.True(
            bounds.X + bounds.Width <= viewportWidth + tolerance,
            $"Element right edge {bounds.X + bounds.Width} exceeded viewport width {viewportWidth} for {viewportName}.");
        Assert.True(
            bounds.Y + bounds.Height <= viewportHeight + tolerance,
            $"Element bottom edge {bounds.Y + bounds.Height} exceeded viewport height {viewportHeight} for {viewportName}.");
    }
}
