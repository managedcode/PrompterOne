using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

[Collection(EditorAuthoringCollection.Name)]
public sealed class EditorToolbarSemanticVisualTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task EditorScreen_FloatingToolbarUsesTwoSemanticDotsAndDistinctTriggerColors()
    {
        UiScenarioArtifacts.ResetScenario(BrowserTestConstants.EditorFlow.ToolbarSemanticScenario);

        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page)).ToBeVisibleAsync(new()
            {
                Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs
            });

            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetSelectionByTextAsync(page, BrowserTestConstants.Editor.DemoFloatingSelectionTarget);

            var floatingBar = page.GetByTestId(UiTestIds.Editor.FloatingBar);
            await Expect(floatingBar).ToBeVisibleAsync();
            await page.WaitForTimeoutAsync(BrowserTestConstants.Timing.FloatingToolbarSettleDelayMs);
            await UiScenarioArtifacts.CaptureLocatorAsync(
                floatingBar,
                BrowserTestConstants.EditorFlow.ToolbarSemanticScenario,
                BrowserTestConstants.EditorFlow.ToolbarSemanticStep);

            var metrics = await floatingBar.EvaluateAsync<FloatingToolbarSemanticMetrics>(
                """
                (element, args) => {
                    const selectChannels = value => (value?.match(/\d+(\.\d+)?/g) ?? []).slice(0, 3).map(Number);
                    const distance = (a, b) => a.length < 3 || b.length < 3
                        ? 0
                        : Math.abs(a[0] - b[0]) + Math.abs(a[1] - b[1]) + Math.abs(a[2] - b[2]);
                    const colorDistance = (firstNode, secondNode, propertyName = 'color') =>
                        distance(
                            selectChannels(window.getComputedStyle(firstNode ?? document.body)[propertyName]),
                            selectChannels(window.getComputedStyle(secondNode ?? document.body)[propertyName]));
                    const byTestId = testId => document.querySelector(`[data-testid="${testId}"]`);

                    const loudButton = element.querySelector(`[data-testid="${args.loudTestId}"]`);
                    const voiceButton = element.querySelector(`[data-testid="${args.floatingVoiceTestId}"]`);
                    const emotionButton = element.querySelector(`[data-testid="${args.floatingEmotionTestId}"]`);
                    const topVoiceButton = byTestId(args.topVoiceTestId);
                    const topEmotionButton = byTestId(args.topEmotionTestId);
                    const topPauseButton = byTestId(args.topPauseTestId);
                    const floatingPauseButton = element.querySelector(`[data-testid="${args.floatingPauseTestId}"]`);
                    const topSpeedButton = byTestId(args.topSpeedTestId);
                    const floatingSpeedButton = element.querySelector(`[data-testid="${args.floatingSpeedTestId}"]`);
                    const topInsertButton = byTestId(args.topInsertTestId);
                    const floatingInsertButton = element.querySelector(`[data-testid="${args.floatingInsertTestId}"]`);

                    return {
                        floatingDotCount: element.querySelectorAll('.efb-sem-dot').length,
                        loudUsesDot: Boolean(loudButton?.querySelector('.cdot, .efb-sem-dot, .tb-sem-dot')),
                        loudUsesSvg: Boolean(loudButton?.querySelector('svg')),
                        voiceColorDistance: colorDistance(topVoiceButton?.querySelector('.tb-sem-dot'), voiceButton?.querySelector('.efb-sem-dot'), 'backgroundColor'),
                        emotionColorDistance: colorDistance(topEmotionButton?.querySelector('.tb-sem-dot'), emotionButton?.querySelector('.efb-sem-dot'), 'backgroundColor'),
                        topGroupColorDistance: colorDistance(topVoiceButton?.querySelector('.tb-sem-dot'), topEmotionButton?.querySelector('.tb-sem-dot'), 'backgroundColor'),
                        floatingGroupColorDistance: colorDistance(voiceButton?.querySelector('.efb-sem-dot'), emotionButton?.querySelector('.efb-sem-dot'), 'backgroundColor'),
                        pauseColorDistance: colorDistance(topPauseButton, floatingPauseButton),
                        speedColorDistance: colorDistance(topSpeedButton, floatingSpeedButton),
                        insertColorDistance: colorDistance(topInsertButton, floatingInsertButton)
                    };
                }
                """,
                new
                {
                    loudTestId = UiTestIds.Editor.FloatingVoiceLoud,
                    floatingEmotionTestId = UiTestIds.Editor.FloatingEmotion,
                    floatingInsertTestId = UiTestIds.Editor.FloatingInsert,
                    floatingPauseTestId = UiTestIds.Editor.FloatingPauseTrigger,
                    floatingSpeedTestId = UiTestIds.Editor.FloatingSpeedTrigger,
                    floatingVoiceTestId = UiTestIds.Editor.FloatingVoice,
                    topEmotionTestId = UiTestIds.Editor.EmotionTrigger,
                    topInsertTestId = UiTestIds.Editor.InsertTrigger,
                    topPauseTestId = UiTestIds.Editor.PauseTrigger,
                    topSpeedTestId = UiTestIds.Editor.SpeedTrigger,
                    topVoiceTestId = UiTestIds.Editor.ColorTrigger
                });

            Assert.Equal(BrowserTestConstants.EditorFlow.FloatingSemanticDotCount, metrics.FloatingDotCount);
            Assert.False(metrics.LoudUsesDot);
            Assert.True(metrics.LoudUsesSvg);
            Assert.True(metrics.VoiceColorDistance >= BrowserTestConstants.EditorFlow.MinimumSemanticColorDistance);
            Assert.True(metrics.EmotionColorDistance >= BrowserTestConstants.EditorFlow.MinimumSemanticColorDistance);
            Assert.True(metrics.TopGroupColorDistance >= BrowserTestConstants.EditorFlow.MinimumSemanticGroupColorDistance);
            Assert.True(metrics.FloatingGroupColorDistance >= BrowserTestConstants.EditorFlow.MinimumSemanticGroupColorDistance);
            Assert.True(metrics.PauseColorDistance >= BrowserTestConstants.EditorFlow.MinimumSemanticColorDistance);
            Assert.True(metrics.SpeedColorDistance >= BrowserTestConstants.EditorFlow.MinimumSemanticColorDistance);
            Assert.True(metrics.InsertColorDistance >= BrowserTestConstants.EditorFlow.MinimumSemanticColorDistance);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_VoiceDropdown_UsesEvenRowRhythm_AndClearSurfaceBorder()
    {
        UiScenarioArtifacts.ResetScenario(BrowserTestConstants.EditorFlow.ToolbarSurfaceScenario);

        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page)).ToBeVisibleAsync(new()
            {
                Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs
            });

            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await page.GetByTestId(UiTestIds.Editor.ColorTrigger).ClickAsync();

            var voiceMenu = page.GetByTestId(UiTestIds.Editor.MenuColor);
            await Expect(voiceMenu).ToBeVisibleAsync();
            await UiScenarioArtifacts.CaptureLocatorAsync(
                voiceMenu,
                BrowserTestConstants.EditorFlow.ToolbarSurfaceScenario,
                BrowserTestConstants.EditorFlow.ToolbarSurfaceStep);

            var metrics = await voiceMenu.EvaluateAsync<DropdownSurfaceMetrics>(
                """
                (element, args) => {
                    const parseColor = value => (value?.match(/\d+(\.\d+)?/g) ?? []).map(Number);
                    const readAlpha = value => {
                        const channels = parseColor(value);
                        return channels.length >= 4 ? channels[3] : 1;
                    };
                    const distance = (left, right) =>
                        left.length < 3 || right.length < 3
                            ? 0
                            : Math.abs(left[0] - right[0]) + Math.abs(left[1] - right[1]) + Math.abs(left[2] - right[2]);
                    const heights = args.itemTestIds
                        .map(testId => document.querySelector(`[data-testid="${testId}"]`))
                        .filter(Boolean)
                        .map(item => item.getBoundingClientRect().height);
                    const styles = window.getComputedStyle(element);

                    return {
                        menuWidth: element.getBoundingClientRect().width,
                        rowHeightDelta: heights.length === 0 ? 0 : Math.max(...heights) - Math.min(...heights),
                        borderAlpha: readAlpha(styles.borderTopColor),
                        borderContrast: distance(parseColor(styles.borderTopColor), parseColor(styles.backgroundColor))
                    };
                }
                """,
                new
                {
                    itemTestIds = new[]
                    {
                        UiTestIds.Editor.ColorLoud,
                        UiTestIds.Editor.ColorSoft,
                        UiTestIds.Editor.ColorWhisper,
                        UiTestIds.Editor.ColorStress,
                        UiTestIds.Editor.ColorGuide
                    }
                });

            Assert.True(metrics.MenuWidth >= BrowserTestConstants.EditorFlow.MinimumDropdownSurfaceWidthPx);
            Assert.True(metrics.RowHeightDelta <= BrowserTestConstants.EditorFlow.MaximumDropdownRowHeightDeltaPx);
            Assert.True(metrics.BorderAlpha >= BrowserTestConstants.EditorFlow.MinimumDropdownSurfaceBorderAlpha);
            Assert.True(metrics.BorderContrast > 0);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private readonly record struct FloatingToolbarSemanticMetrics(
        int FloatingDotCount,
        bool LoudUsesDot,
        bool LoudUsesSvg,
        double VoiceColorDistance,
        double EmotionColorDistance,
        double TopGroupColorDistance,
        double FloatingGroupColorDistance,
        double PauseColorDistance,
        double SpeedColorDistance,
        double InsertColorDistance);

    private readonly record struct DropdownSurfaceMetrics(
        double MenuWidth,
        double RowHeightDelta,
        double BorderAlpha,
        double BorderContrast);
}
