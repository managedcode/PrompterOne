using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class EditorMinimapLayoutTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task EditorScreen_MonacoMinimapStaysVisibleInsideEditorStage()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await EditorMonacoDriver.WaitUntilReadyAsync(page);

            var stage = page.GetByTestId(UiTestIds.Editor.SourceStage);
            var minimap = page.GetByTestId(UiTestIds.Editor.SourceMinimap);
            var state = await EditorMonacoDriver.GetStateAsync(page);
            var stageBounds = await GetRequiredBoundingBoxAsync(stage);
            await Expect(minimap)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });

            var minimapBounds = await GetRequiredBoundingBoxAsync(minimap);
            var stageRight = stageBounds.X + stageBounds.Width;

            Assert.True(
                state.Layout.MinimapWidth >= BrowserTestConstants.Editor.MinimapMinimumWidthPx,
                $"Expected Monaco minimap width to be at least {BrowserTestConstants.Editor.MinimapMinimumWidthPx}px, but was {state.Layout.MinimapWidth:0.##}px.");
            Assert.InRange(
                minimapBounds.X - stageBounds.X,
                0,
                stageBounds.Width);
            Assert.InRange(
                stageRight - (minimapBounds.X + minimapBounds.Width),
                0,
                BrowserTestConstants.Editor.MinimapStageEdgeTolerancePx);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static async Task<LayoutBounds> GetRequiredBoundingBoxAsync(ILocator locator) =>
        await locator.EvaluateAsync<LayoutBounds>(
            """
            element => {
                const rect = element.getBoundingClientRect();
                return {
                    x: rect.x,
                    y: rect.y,
                    width: rect.width,
                    height: rect.height
                };
            }
            """);

    private readonly record struct LayoutBounds(double X, double Y, double Width, double Height);
}
