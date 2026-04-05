using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Services.Editor;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

public sealed class EditorCueRenderingFlowTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private const string CueScenario = "editor-tps-cue-rendering";
    private const string OverlayStepName = "01-editor-cue-overlay";
    private const string MonacoStylingStepName = "02-editor-monaco-styling";

    [Fact]
    public async Task EditorScreen_RendersCueAwareOverlayContractsForDeliveryPreview()
    {
        UiScenarioArtifacts.ResetScenario(CueScenario);

        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetTextAsync(
                page,
                """
                ## [Cue Demo|140WPM|neutral]
                ### [Delivery Block|140WPM|neutral]
                [loud][building]Rise together[/building][/loud] and [soft]listen[stress]ing[/stress][/soft].
                """);

            var highlight = page.GetByTestId(UiTestIds.Editor.SourceHighlight);
            var probe = await highlight.EvaluateAsync<EditorCueProbe>(
                $$"""
                host => {
                    const loud = host.querySelector('[{{TpsVisualCueContracts.VolumeAttributeName}}="{{TpsVisualCueContracts.VolumeLoud}}"]');
                    const soft = host.querySelector('[{{TpsVisualCueContracts.VolumeAttributeName}}="{{TpsVisualCueContracts.VolumeSoft}}"]');
                    const building = host.querySelector('[{{TpsVisualCueContracts.DeliveryAttributeName}}="{{TpsVisualCueContracts.DeliveryModeBuilding}}"]');
                    const stress = host.querySelector('[{{TpsVisualCueContracts.StressAttributeName}}="{{TpsVisualCueContracts.StressAttributeValue}}"]');

                    const readScale = element => {
                        if (!(element instanceof HTMLElement)) {
                            return '';
                        }

                        return getComputedStyle(element).getPropertyValue('{{TpsVisualCueContracts.CueScaleVariableName}}').trim();
                    };

                    return {
                        loudVolume: loud?.getAttribute('{{TpsVisualCueContracts.VolumeAttributeName}}') ?? '',
                        softVolume: soft?.getAttribute('{{TpsVisualCueContracts.VolumeAttributeName}}') ?? '',
                        buildingDelivery: building?.getAttribute('{{TpsVisualCueContracts.DeliveryAttributeName}}') ?? '',
                        stressValue: stress?.getAttribute('{{TpsVisualCueContracts.StressAttributeName}}') ?? '',
                        loudScale: readScale(loud),
                        softScale: readScale(soft)
                    };
                }
                """);

            Assert.Equal(TpsVisualCueContracts.VolumeLoud, probe.LoudVolume);
            Assert.Equal(TpsVisualCueContracts.VolumeSoft, probe.SoftVolume);
            Assert.Equal(TpsVisualCueContracts.DeliveryModeBuilding, probe.BuildingDelivery);
            Assert.Equal(TpsVisualCueContracts.StressAttributeValue, probe.StressValue);
            Assert.False(string.IsNullOrWhiteSpace(probe.LoudScale));
            Assert.False(string.IsNullOrWhiteSpace(probe.SoftScale));

            await UiScenarioArtifacts.CapturePageAsync(page, CueScenario, OverlayStepName);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_RendersMonacoCueStylesImmediatelyAfterImport()
    {
        UiScenarioArtifacts.ResetScenario(CueScenario);

        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetTextAsync(
                page,
                """
                ## [Cue Import|140WPM|Professional]
                ### [Delivery Block|140WPM|Warm]
                [loud][building]Rise together[/building][/loud] and [soft][emphasis]listen closely[/emphasis][/soft]. //
                [pronunciation:TELE-promp-ter]teleprompter[/pronunciation] [highlight]tonight[/highlight]
                """);

            await page.WaitForFunctionAsync(
                """
                (args) => {
                    const harness = window[args.harnessGlobalName];
                    const state = harness?.getState(args.stageTestId);
                    const classes = state?.decorationClasses ?? [];
                    return classes.some(value => value.includes(args.emphasisClass)) &&
                        classes.some(value => value.includes(args.highlightClass)) &&
                        classes.some(value => value.includes(args.loudClass)) &&
                        classes.some(value => value.includes(args.pauseClass)) &&
                        classes.some(value => value.includes(args.pronunciationClass)) &&
                        classes.some(value => value.includes(args.headerEmotionClass));
                }
                """,
                new
                {
                    emphasisClass = "po-inline-emphasis",
                    harnessGlobalName = EditorMonacoRuntimeContract.BrowserHarnessGlobalName,
                    headerEmotionClass = "po-header-emotion",
                    highlightClass = "po-inline-highlight",
                    loudClass = "po-inline-loud",
                    pauseClass = "po-pause-long",
                    pronunciationClass = "po-inline-pronunciation-word",
                    stageTestId = UiTestIds.Editor.SourceStage
                },
                new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });

            var stage = page.GetByTestId(UiTestIds.Editor.SourceStage);
            var probe = await stage.EvaluateAsync<EditorMonacoCueProbe>(
                """
                (host) => {
                    const readStyle = (selector, propertyName) => {
                        const element = host.querySelector(selector);
                        if (!(element instanceof HTMLElement)) {
                            return '';
                        }

                        return getComputedStyle(element).getPropertyValue(propertyName).trim();
                    };

                    return {
                        emphasisTextDecoration: readStyle('.po-inline-emphasis', 'text-decoration-line'),
                        headerEmotionText: host.querySelector('.po-header-emotion')?.textContent?.trim() ?? '',
                        highlightBackgroundImage: readStyle('.po-inline-highlight', 'background-image'),
                        loudDisplay: readStyle('.po-inline-loud', 'display'),
                        loudTransform: readStyle('.po-inline-loud', 'transform'),
                        pauseColor: readStyle('.po-pause-long', 'color'),
                        pronunciationBorderStyle: readStyle('.po-inline-pronunciation-word', 'border-bottom-style'),
                        tagColor: readStyle('.po-tag', 'color')
                    };
                }
                """);

            Assert.Equal("Professional", probe.HeaderEmotionText);
            Assert.Contains("underline", probe.EmphasisTextDecoration, StringComparison.Ordinal);
            Assert.NotEqual("none", probe.HighlightBackgroundImage);
            Assert.Equal("inline-block", probe.LoudDisplay);
            Assert.NotEqual("none", probe.LoudTransform);
            Assert.Equal("dashed", probe.PronunciationBorderStyle);
            Assert.False(string.IsNullOrWhiteSpace(probe.PauseColor));
            Assert.False(string.IsNullOrWhiteSpace(probe.TagColor));

            await UiScenarioArtifacts.CapturePageAsync(page, CueScenario, MonacoStylingStepName);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private sealed class EditorCueProbe
    {
        public string LoudVolume { get; init; } = string.Empty;

        public string SoftVolume { get; init; } = string.Empty;

        public string BuildingDelivery { get; init; } = string.Empty;

        public string StressValue { get; init; } = string.Empty;

        public string LoudScale { get; init; } = string.Empty;

        public string SoftScale { get; init; } = string.Empty;
    }

    private sealed class EditorMonacoCueProbe
    {
        public string EmphasisTextDecoration { get; init; } = string.Empty;

        public string HeaderEmotionText { get; init; } = string.Empty;

        public string HighlightBackgroundImage { get; init; } = string.Empty;

        public string LoudDisplay { get; init; } = string.Empty;

        public string LoudTransform { get; init; } = string.Empty;

        public string PauseColor { get; init; } = string.Empty;

        public string PronunciationBorderStyle { get; init; } = string.Empty;

        public string TagColor { get; init; } = string.Empty;
    }
}
