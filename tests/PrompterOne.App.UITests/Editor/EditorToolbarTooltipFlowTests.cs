using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class EditorToolbarTooltipFlowTests(StandaloneAppFixture fixture) : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    private const string EmotionTooltipText =
        "Emotion — applies mood-based color styling and presentation hints. Used on segments, blocks, or inline text";
    private const string MotivationalTooltipText =
        "Inspiring, encouraging. Inline: [motivational]text[/motivational]";
    private const string WrappedTooltipSelectionFragment = "[motivational]script[/motivational]";

    private readonly record struct ElementBounds(double Left, double Top, double Right, double Bottom);

    [Fact]
    public Task EditorScreen_ToolbarTooltip_AppearsOnlyAfterDelay() =>
        RunPageAsync(async page =>
        {
            UiScenarioArtifacts.ResetScenario(BrowserTestConstants.EditorFlow.ToolbarTooltipScenario);

            await OpenEditorAsync(page);

            var emotionTrigger = page.GetByTestId(UiTestIds.Editor.EmotionTrigger);
            var tooltip = GetTooltipLocator(page, EmotionTooltipText);

            await emotionTrigger.HoverAsync();
            await page.WaitForTimeoutAsync(BrowserTestConstants.EditorFlow.TooltipEarlyCheckDelayMs);

            Assert.InRange(
                await ReadOpacityAsync(tooltip),
                0,
                BrowserTestConstants.EditorFlow.MaximumEarlyTooltipOpacity);

            await page.WaitForTimeoutAsync(
                BrowserTestConstants.EditorFlow.TooltipSettleDelayMs - BrowserTestConstants.EditorFlow.TooltipEarlyCheckDelayMs);
            await Expect(tooltip).ToBeVisibleAsync();
            await Expect(tooltip).ToHaveTextAsync(EmotionTooltipText);

            await UiScenarioArtifacts.CapturePageAsync(
                page,
                BrowserTestConstants.EditorFlow.ToolbarTooltipScenario,
                BrowserTestConstants.EditorFlow.ToolbarTooltipDelayStep);
        });

    [Fact]
    public Task EditorScreen_DropdownTooltip_StaysOutsideMenuAndDoesNotBlockAction() =>
        RunPageAsync(async page =>
        {
            UiScenarioArtifacts.ResetScenario(BrowserTestConstants.EditorFlow.ToolbarTooltipScenario);

            await OpenEditorAsync(page);
            await EditorMonacoDriver.SetTextAsync(page, BrowserTestConstants.Editor.TypedScript);
            await EditorMonacoDriver.SetSelectionByTextAsync(page, BrowserTestConstants.Editor.TypedSelectionTarget);

            var emotionTrigger = page.GetByTestId(UiTestIds.Editor.EmotionTrigger);
            var emotionMenu = page.GetByTestId(UiTestIds.Editor.MenuEmotion);
            var motivationalEmotion = page.GetByTestId(UiTestIds.Editor.EmotionMotivational);
            var tooltip = GetTooltipLocator(page, MotivationalTooltipText);

            await emotionTrigger.ClickAsync();
            await Expect(emotionMenu).ToBeVisibleAsync();
            await motivationalEmotion.HoverAsync();
            await page.WaitForTimeoutAsync(BrowserTestConstants.EditorFlow.TooltipEarlyCheckDelayMs);

            Assert.InRange(
                await ReadOpacityAsync(tooltip),
                0,
                BrowserTestConstants.EditorFlow.MaximumEarlyTooltipOpacity);

            await page.WaitForTimeoutAsync(
                BrowserTestConstants.EditorFlow.TooltipSettleDelayMs - BrowserTestConstants.EditorFlow.TooltipEarlyCheckDelayMs);
            await Expect(tooltip).ToBeVisibleAsync();
            await Expect(tooltip).ToHaveTextAsync(MotivationalTooltipText);

            var menuBounds = await ReadBoundsAsync(emotionMenu);
            var tooltipBounds = await ReadBoundsAsync(tooltip);
            var overlap = CalculateIntersectionArea(menuBounds, tooltipBounds);

            Assert.InRange(overlap, 0, BrowserTestConstants.EditorFlow.MaximumTooltipMenuOverlapPx);

            await motivationalEmotion.ClickAsync();
            await Expect(EditorMonacoDriver.SourceInput(page))
                .ToHaveValueAsync(new Regex(Regex.Escape(WrappedTooltipSelectionFragment)));

            await UiScenarioArtifacts.CapturePageAsync(
                page,
                BrowserTestConstants.EditorFlow.ToolbarTooltipScenario,
                BrowserTestConstants.EditorFlow.ToolbarTooltipDropdownStep);
        });

    private static double CalculateIntersectionArea(ElementBounds left, ElementBounds right)
    {
        var overlapWidth = Math.Max(0, Math.Min(left.Right, right.Right) - Math.Max(left.Left, right.Left));
        var overlapHeight = Math.Max(0, Math.Min(left.Bottom, right.Bottom) - Math.Max(left.Top, right.Top));
        return overlapWidth * overlapHeight;
    }

    private static ILocator GetTooltipLocator(IPage page, string tooltipText) =>
        page.GetByTestId(UiTestIds.Editor.ToolbarTooltip)
            .Filter(new() { HasTextString = tooltipText });

    private static async Task<double> ReadOpacityAsync(ILocator locator) =>
        await locator.EvaluateAsync<double>(
            """
            element => Number.parseFloat(getComputedStyle(element).opacity)
            """);

    private static async Task OpenEditorAsync(IPage page)
    {
        await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
        await Expect(page.GetByTestId(UiTestIds.Editor.Page))
            .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
        await EditorMonacoDriver.WaitUntilReadyAsync(page);
    }

    private static async Task<ElementBounds> ReadBoundsAsync(ILocator locator) =>
        await locator.EvaluateAsync<ElementBounds>(
            """
            element => {
                const rect = element.getBoundingClientRect();
                return {
                    left: rect.left,
                    top: rect.top,
                    right: rect.right,
                    bottom: rect.bottom
                };
            }
            """);
}
