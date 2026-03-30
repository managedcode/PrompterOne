using PrompterLive.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterLive.App.UITests;

public sealed class LearnFidelityTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly record struct ContextGapMeasurement(double LeftGapPx, double RightGapPx);
    private readonly record struct VisibleContextWordGapMeasurement(double LeftWordGapPx, double RightWordGapPx);

    [Fact]
    public async Task LearnScreen_KeepsOrpLetterCenteredOnReferenceGuide()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.LearnQuantum);
            await Expect(page.GetByTestId(UiTestIds.Learn.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
            await Expect(page.GetByTestId(UiTestIds.Learn.Word).Locator(".orp")).ToBeVisibleAsync();

            var initialDelta = await MeasureOrpDeltaAsync(page);
            Assert.InRange(initialDelta, 0, 6);

            await page.GetByTestId(UiTestIds.Learn.PlayToggle).ClickAsync();
            await page.WaitForTimeoutAsync(BrowserTestConstants.Timing.LearnPlaybackDelayMs);

            var playbackDelta = await MeasureOrpDeltaAsync(page);
            Assert.InRange(playbackDelta, 0, 6);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task LearnScreen_UsesPhraseTimelineForSecurityIncidentScript()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.LearnSecurityIncident);
            await Expect(page.GetByTestId(UiTestIds.Learn.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await page.GetByTestId(UiTestIds.Learn.StepForwardLarge).ClickAsync();

            for (var index = 0; index < BrowserTestConstants.Learn.MidFlowStepSmall; index++)
            {
                await page.GetByTestId(UiTestIds.Learn.StepForward).ClickAsync();
            }

            await Expect(page.GetByTestId(UiTestIds.Learn.Word))
                .ToHaveTextAsync(BrowserTestConstants.Learn.MidFlowWord);
            await Expect(page.GetByTestId(UiTestIds.Learn.NextPhrase))
                .Not.ToHaveTextAsync(BrowserTestConstants.Learn.EndOfScriptText);
            await Expect(page.GetByTestId(UiTestIds.Learn.NextPhrase))
                .ToContainTextAsync(BrowserTestConstants.Learn.NextPhraseFragment);

            var leftContextCount = await CountContextWordsAsync(page, UiDomIds.Learn.ContextLeft);
            var rightContextCount = await CountContextWordsAsync(page, UiDomIds.Learn.ContextRight);

            Assert.Equal(BrowserTestConstants.Learn.ContextWordCount, leftContextCount);
            Assert.Equal(BrowserTestConstants.Learn.ContextWordCount, rightContextCount);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task LearnScreen_KeepsContextRailsSeparatedFromFocusWordOnLeadershipScript()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.SetViewportSizeAsync(
                BrowserTestConstants.Learn.OverlapViewportWidth,
                BrowserTestConstants.Learn.OverlapViewportHeight);
            await page.GotoAsync(BrowserTestConstants.Routes.LearnLeadership);
            await Expect(page.GetByTestId(UiTestIds.Learn.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await StepUntilWordAsync(page, BrowserTestConstants.Learn.OverlapProbeWord, BrowserTestConstants.Learn.OverlapProbeStepLimit);

            var gaps = await MeasureContextGapsAsync(page);

            Assert.True(
                gaps.LeftGapPx >= 0,
                $"Expected the left context rail to stop before the focus word, but the overlap was {-gaps.LeftGapPx:0.##} px.");
            Assert.True(
                gaps.RightGapPx >= 0,
                $"Expected the right context rail to start after the focus word, but the overlap was {-gaps.RightGapPx:0.##} px.");
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task LearnScreen_KeepsSecurityIncidentContextWordsCloseToFocusedWord()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.SetViewportSizeAsync(
                BrowserTestConstants.Learn.SecurityIncidentViewportWidth,
                BrowserTestConstants.Learn.SecurityIncidentViewportHeight);
            await page.GotoAsync(BrowserTestConstants.Routes.LearnSecurityIncident);
            await Expect(page.GetByTestId(UiTestIds.Learn.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await StepUntilWordAsync(
                page,
                BrowserTestConstants.Learn.SecurityIncidentProbeWord,
                BrowserTestConstants.Learn.SecurityIncidentProbeStepLimit);

            var gaps = await MeasureVisibleContextWordGapsAsync(page);

            Assert.True(
                gaps.LeftWordGapPx <= BrowserTestConstants.Learn.MaxVisibleContextWordGapPx,
                $"Expected the visible left context word gap to stay within {BrowserTestConstants.Learn.MaxVisibleContextWordGapPx}px, but it was {gaps.LeftWordGapPx:0.##}px.");
            Assert.True(
                gaps.RightWordGapPx <= BrowserTestConstants.Learn.MaxVisibleContextWordGapPx,
                $"Expected the visible right context word gap to stay within {BrowserTestConstants.Learn.MaxVisibleContextWordGapPx}px, but it was {gaps.RightWordGapPx:0.##}px.");
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static Task<double> MeasureOrpDeltaAsync(Microsoft.Playwright.IPage page) =>
        page.EvaluateAsync<double>(
            """
            () => {
                const line = document.querySelector('[data-testid="learn-orp-line"]');
                const orp = document.querySelector('[data-testid="learn-word"] .orp');
                if (!line || !orp) {
                    return 999;
                }

                const lineRect = line.getBoundingClientRect();
                const orpRect = orp.getBoundingClientRect();
                const lineCenter = lineRect.left + (lineRect.width / 2);
                const orpCenter = orpRect.left + (orpRect.width / 2);
                return Math.abs(lineCenter - orpCenter);
            }
            """);

    private static Task<int> CountContextWordsAsync(Microsoft.Playwright.IPage page, string elementId) =>
        page.EvaluateAsync<int>(
            """
            targetId => {
                const element = document.getElementById(targetId);
                return element ? element.children.length : -1;
            }
            """,
            elementId);

    private static async Task StepUntilWordAsync(Microsoft.Playwright.IPage page, string targetWord, int stepLimit)
    {
        for (var stepIndex = 0; stepIndex < stepLimit; stepIndex++)
        {
            var currentWord = await page.GetByTestId(UiTestIds.Learn.Word).InnerTextAsync();
            if (string.Equals(currentWord.Trim(), targetWord, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await page.GetByTestId(UiTestIds.Learn.StepForward).ClickAsync();
        }

        Assert.Fail($"Did not reach the learn probe word '{targetWord}' within {stepLimit} steps.");
    }

    private static Task<ContextGapMeasurement> MeasureContextGapsAsync(Microsoft.Playwright.IPage page) =>
        page.EvaluateAsync<ContextGapMeasurement>(
            """
            () => {
                const left = document.querySelector('[data-testid="learn-context-left"]');
                const focus = document.querySelector('[data-testid="learn-word"]');
                const right = document.querySelector('[data-testid="learn-context-right"]');
                if (!left || !focus || !right) {
                    return { leftGapPx: -999, rightGapPx: -999 };
                }

                const leftRect = left.getBoundingClientRect();
                const focusRect = focus.getBoundingClientRect();
                const rightRect = right.getBoundingClientRect();

                return {
                    leftGapPx: focusRect.left - leftRect.right,
                    rightGapPx: rightRect.left - focusRect.right
                };
            }
            """);

    private static Task<VisibleContextWordGapMeasurement> MeasureVisibleContextWordGapsAsync(Microsoft.Playwright.IPage page) =>
        page.EvaluateAsync<VisibleContextWordGapMeasurement>(
            """
            ids => {
                const left = document.querySelector(`[data-testid="${ids.left}"]`);
                const focus = document.querySelector(`[data-testid="${ids.focus}"]`);
                const right = document.querySelector(`[data-testid="${ids.right}"]`);
                const leftWord = left?.lastElementChild;
                const rightWord = right?.firstElementChild;
                if (!leftWord || !focus || !rightWord) {
                    return { leftWordGapPx: 999, rightWordGapPx: 999 };
                }

                const leftWordRect = leftWord.getBoundingClientRect();
                const focusRect = focus.getBoundingClientRect();
                const rightWordRect = rightWord.getBoundingClientRect();

                return {
                    leftWordGapPx: focusRect.left - leftWordRect.right,
                    rightWordGapPx: rightWordRect.left - focusRect.right
                };
            }
            """,
            new
            {
                left = UiTestIds.Learn.ContextLeft,
                focus = UiTestIds.Learn.Word,
                right = UiTestIds.Learn.ContextRight
            });
}
