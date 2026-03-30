using Microsoft.Playwright;
using PrompterLive.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterLive.App.UITests;

public sealed class TeleprompterPlaybackContinuityTests(StandaloneAppFixture fixture)
    : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    [Fact]
    public Task Teleprompter_PlaybackContinuesAfterManualBlockJump() =>
        RunPageAsync(async page =>
        {
            await OpenLeadershipTeleprompterAsync(page);
            await StartPlaybackAsync(page);

            await page.GetByTestId(UiTestIds.Teleprompter.NextBlock).ClickAsync();
            await Expect(page.Locator($"#{UiDomIds.Teleprompter.BlockIndicator}"))
                .ToHaveTextAsync(BrowserTestConstants.Regexes.ReaderSecondBlockIndicator);

            await AssertReaderTimeContinuesAdvancingAsync(page);
        });

    [Fact]
    public Task Teleprompter_PlaybackContinuesAfterSliderAdjustmentAndAutomaticCardTransition() =>
        RunPageAsync(async page =>
        {
            await OpenLeadershipTeleprompterAsync(page);
            await SetRangeValueAsync(page.GetByTestId(UiTestIds.Teleprompter.WidthSlider), BrowserTestConstants.ReaderWorkflow.TeleprompterWidth);
            await SetRangeValueAsync(page.GetByTestId(UiTestIds.Teleprompter.FocalSlider), BrowserTestConstants.ReaderWorkflow.TeleprompterFocal);
            await StartPlaybackAsync(page);

            await Expect(page.Locator($"#{UiDomIds.Teleprompter.BlockIndicator}"))
                .ToHaveTextAsync(
                    BrowserTestConstants.Regexes.ReaderSecondBlockIndicator,
                    new() { Timeout = BrowserTestConstants.Timing.ReaderAutomaticTransitionTimeoutMs });

            await AssertReaderTimeContinuesAdvancingAsync(page);
        });

    private static async Task OpenLeadershipTeleprompterAsync(IPage page)
    {
        await page.GotoAsync(BrowserTestConstants.Routes.TeleprompterLeadership);
        await Expect(page.GetByTestId(UiTestIds.Teleprompter.Page))
            .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
    }

    private static async Task StartPlaybackAsync(IPage page)
    {
        await page.GetByTestId(UiTestIds.Teleprompter.PlayToggle).ClickAsync();
        await Expect(page.Locator($"#{UiDomIds.Teleprompter.Time}"))
            .Not.ToHaveTextAsync(
                BrowserTestConstants.Regexes.ReaderTimeNotZero,
                new() { Timeout = BrowserTestConstants.Timing.ReaderPlaybackStartTimeoutMs });
    }

    private static async Task AssertReaderTimeContinuesAdvancingAsync(IPage page)
    {
        await page.WaitForTimeoutAsync(BrowserTestConstants.Timing.ReaderTransitionSettleDelayMs);
        var timeLocator = page.Locator($"#{UiDomIds.Teleprompter.Time}");
        var timeAfterTransition = await timeLocator.TextContentAsync() ?? string.Empty;

        await page.WaitForTimeoutAsync(BrowserTestConstants.Timing.ReaderPostTransitionAdvanceDelayMs);
        await Expect(timeLocator).Not.ToHaveTextAsync(timeAfterTransition);
    }

    private static Task SetRangeValueAsync(ILocator locator, string value) =>
        locator.EvaluateAsync(
            """
            (element, nextValue) => {
                element.value = nextValue;
                element.dispatchEvent(new Event("input", { bubbles: true }));
                element.dispatchEvent(new Event("change", { bubbles: true }));
            }
            """,
            value);
}
