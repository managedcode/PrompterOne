using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

public sealed class EditorLineNumberLayoutTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task EditorScreen_MonacoLineNumbersRenderInsideVisibleGutter()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            UiScenarioArtifacts.ResetScenario(BrowserTestConstants.EditorFlow.LineNumbersScenario);

            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await EditorMonacoDriver.WaitUntilReadyAsync(page);

            var stage = page.GetByTestId(UiTestIds.Editor.SourceStage);
            var gutter = page.GetByTestId(UiTestIds.Editor.SourceGutter);
            var state = await EditorMonacoDriver.GetStateAsync(page);

            await Expect(gutter)
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            await UiScenarioArtifacts.CaptureLocatorAsync(
                gutter,
                BrowserTestConstants.EditorFlow.LineNumbersScenario,
                BrowserTestConstants.EditorFlow.LineNumbersStep);

            var stageBounds = await GetRequiredBoundingBoxAsync(stage);
            var gutterBounds = await GetRequiredBoundingBoxAsync(gutter);
            var gutterText = (await gutter.TextContentAsync())?.Trim() ?? string.Empty;

            Assert.Contains(BrowserTestConstants.Editor.GutterFirstLineNumberText, gutterText, StringComparison.Ordinal);
            Assert.InRange(
                gutterBounds.Width,
                BrowserTestConstants.Editor.GutterMinimumWidthPx,
                BrowserTestConstants.Editor.GutterMaximumWidthPx);
            Assert.InRange(
                gutterBounds.X - stageBounds.X,
                0,
                stageBounds.Width);
            Assert.True(
                state.Layout.ContentLeft >= BrowserTestConstants.Editor.MinimumContentLeftWithLineNumbersPx,
                $"Expected Monaco contentLeft to include the line-number gutter, but it was {state.Layout.ContentLeft:0.##}.");
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
