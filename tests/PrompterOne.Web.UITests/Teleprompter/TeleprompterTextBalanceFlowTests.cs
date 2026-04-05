using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

public sealed class TeleprompterTextBalanceFlowTests(StandaloneAppFixture fixture)
    : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    private const int OpeningCardIndex = 0;

    [Fact]
    public Task TeleprompterScreen_DefaultLeftAlignment_KeepsTextMassNearStageCenter() =>
        RunPageAsync(async page =>
        {
            await page.GotoAsync(BrowserTestConstants.Routes.TeleprompterDemo);
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.Page)).ToBeVisibleAsync(new()
            {
                Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs
            });
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.ClusterWrap)).ToHaveAttributeAsync(
                BrowserTestConstants.TeleprompterFlow.ReaderTextAlignmentAttribute,
                BrowserTestConstants.TeleprompterFlow.AlignmentLeftValue);

            var balance = await page.GetByTestId(UiTestIds.Teleprompter.CardText(OpeningCardIndex))
                .EvaluateAsync<ReaderOpticalAlignmentProbe?>(
                    """
                    (element, stageId) => {
                        if (!(element instanceof HTMLElement)) {
                            return null;
                        }

                        const stage = document.getElementById(stageId);
                        if (!(stage instanceof HTMLElement)) {
                            return null;
                        }

                        const range = document.createRange();
                        range.selectNodeContents(element);

                        const stageRect = stage.getBoundingClientRect();
                        const wordRects = Array
                            .from(range.getClientRects())
                            .filter(rect => rect.width > 0 && rect.height > 0);

                        if (wordRects.length === 0) {
                            return {
                                lineCount: 0,
                                maxAsymmetryPx: 0,
                                averageAsymmetryPx: 0
                            };
                        }

                        const lineRectMap = new Map();
                        for (const rect of wordRects) {
                            const key = Math.round(rect.top);
                            const existing = lineRectMap.get(key);
                            if (existing) {
                                existing.left = Math.min(existing.left, rect.left);
                                existing.right = Math.max(existing.right, rect.right);
                            } else {
                                lineRectMap.set(key, { left: rect.left, right: rect.right });
                            }
                        }

                        const lineRects = Array.from(lineRectMap.values());
                        const stageCenter = stageRect.left + (stageRect.width / 2);
                        const centerOffsets = lineRects.map(rect => {
                            const lineCenter = rect.left + ((rect.right - rect.left) / 2);
                            return Math.abs(stageCenter - lineCenter);
                        });
                        const computedStyle = window.getComputedStyle(element);

                        return {
                            lineCount: lineRects.length,
                            textAlign: computedStyle.textAlign,
                            paddingInlineStartPx: parseFloat(computedStyle.paddingInlineStart || "0"),
                            maxCenterOffsetPx: Math.max(...centerOffsets),
                            averageCenterOffsetPx: centerOffsets.reduce((sum, value) => sum + value, 0) / centerOffsets.length
                        };
                    }
                    """,
                    UiDomIds.Teleprompter.ClusterWrap);

            Assert.NotNull(balance);
            Assert.Equal(BrowserTestConstants.TeleprompterFlow.AlignmentLeftValue, balance.TextAlign);
            Assert.True(
                balance.LineCount >= BrowserTestConstants.TeleprompterFlow.MinimumBalancedTextLineCount,
                $"Expected at least {BrowserTestConstants.TeleprompterFlow.MinimumBalancedTextLineCount} visible text lines, but found {balance.LineCount}.");
            Assert.InRange(
                balance.PaddingInlineStartPx,
                BrowserTestConstants.TeleprompterFlow.MinimumOpticalInsetPx,
                double.MaxValue);
            Assert.InRange(
                balance.MaxCenterOffsetPx,
                0,
                BrowserTestConstants.TeleprompterFlow.MaximumDefaultLeftLineCenterOffsetPx);
            Assert.InRange(
                balance.AverageCenterOffsetPx,
                0,
                BrowserTestConstants.TeleprompterFlow.MaximumDefaultLeftAverageCenterOffsetPx);
        });

    private sealed class ReaderOpticalAlignmentProbe
    {
        public int LineCount { get; init; }

        public string TextAlign { get; init; } = string.Empty;

        public double PaddingInlineStartPx { get; init; }

        public double MaxCenterOffsetPx { get; init; }

        public double AverageCenterOffsetPx { get; init; }
    }
}
