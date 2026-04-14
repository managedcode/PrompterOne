using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Services.Editor;

namespace PrompterOne.Web.UITests;

[ClassDataSource<StandaloneAppFixture>(Shared = SharedType.PerClass)]
public sealed class EditorCueRenderingFlowTests(StandaloneAppFixture fixture)
{
    private const string CueScenario = "editor-tps-cue-rendering";
    private const string OverlayStepName = "01-editor-cue-overlay";
    private const string MonacoStylingStepName = "02-editor-monaco-styling";

    [Test]
    public async Task EditorScreen_RendersCueAwareOverlayContractsForDeliveryPreview()
    {
        UiScenarioArtifacts.ResetScenario(CueScenario);

        var page = await fixture.NewPageAsync(additionalContext: true);

        try
        {
            await EditorIsolatedDraftDriver.OpenBlankDraftAsync(page);
            await EditorMonacoDriver.SetTextAsync(
                page,
                """
                ## [Cue Demo|140WPM|neutral]
                ### [Delivery Block|140WPM|neutral]
                [loud][building]Rise together[/building][/loud] and [soft]listen[stress]ing[/stress][/soft].
                [breath] [legato][energy:8]steady[/energy][/legato] [staccato][melody:4]rhythm[/melody][/staccato].
                """);
            var probeHandle = await page.WaitForFunctionAsync(
                """
                (args) => {
                    const host = document.querySelector(`[data-test="${args.overlayTestId}"]`);
                    if (!(host instanceof HTMLElement)) {
                        return false;
                    }

                    const nodes = [...host.querySelectorAll('*')];
                    const loud = nodes.find(node =>
                        node?.getAttribute(args.volumeAttributeName) === args.loudValue);
                    const soft = nodes.find(node =>
                        node?.getAttribute(args.volumeAttributeName) === args.softValue);
                    const building = nodes.find(node =>
                        node?.getAttribute(args.deliveryAttributeName) === args.buildingValue);
                    const stress = nodes.find(node =>
                        node?.getAttribute(args.stressAttributeName) === args.stressValue);
                    const legato = nodes.find(node =>
                        node?.getAttribute(args.articulationAttributeName) === args.legatoValue);
                    const staccato = nodes.find(node =>
                        node?.getAttribute(args.articulationAttributeName) === args.staccatoValue);
                    const energy = nodes.find(node =>
                        node?.getAttribute(args.energyAttributeName) === args.energyValue);
                    const melody = nodes.find(node =>
                        node?.getAttribute(args.melodyAttributeName) === args.melodyValue);
                    const breath = nodes.find(node =>
                        node?.getAttribute(args.breathAttributeName) === args.breathValue);
                    const readScale = element =>
                        element instanceof HTMLElement
                            ? getComputedStyle(element).getPropertyValue(args.cueScaleVariableName).trim()
                            : '';

                    const loudScale = readScale(loud);
                    const softScale = readScale(soft);
                    if (!loud || !soft || !building || !stress || !legato || !staccato || !energy || !melody || !breath || !loudScale || !softScale) {
                        return false;
                    }

                    return {
                        loudVolume: loud.getAttribute(args.volumeAttributeName) ?? '',
                        softVolume: soft.getAttribute(args.volumeAttributeName) ?? '',
                        buildingDelivery: building.getAttribute(args.deliveryAttributeName) ?? '',
                        stressValue: stress.getAttribute(args.stressAttributeName) ?? '',
                        legatoArticulation: legato.getAttribute(args.articulationAttributeName) ?? '',
                        staccatoArticulation: staccato.getAttribute(args.articulationAttributeName) ?? '',
                        energyValue: energy.getAttribute(args.energyAttributeName) ?? '',
                        melodyValue: melody.getAttribute(args.melodyAttributeName) ?? '',
                        breathValue: breath.getAttribute(args.breathAttributeName) ?? '',
                        loudScale,
                        softScale
                    };
                }
                """,
                new
                {
                    articulationAttributeName = TpsVisualCueContracts.ArticulationAttributeName,
                    breathAttributeName = TpsVisualCueContracts.BreathAttributeName,
                    breathValue = TpsVisualCueContracts.BreathAttributeValue,
                    cueScaleVariableName = TpsVisualCueContracts.CueScaleVariableName,
                    deliveryAttributeName = TpsVisualCueContracts.DeliveryAttributeName,
                    buildingValue = TpsVisualCueContracts.DeliveryModeBuilding,
                    energyAttributeName = TpsVisualCueContracts.EnergyAttributeName,
                    energyValue = "8",
                    legatoValue = TpsVisualCueContracts.ArticulationLegato,
                    melodyAttributeName = TpsVisualCueContracts.MelodyAttributeName,
                    melodyValue = "4",
                    loudValue = TpsVisualCueContracts.VolumeLoud,
                    overlayTestId = UiTestIds.Editor.SourceHighlight,
                    softValue = TpsVisualCueContracts.VolumeSoft,
                    staccatoValue = TpsVisualCueContracts.ArticulationStaccato,
                    stressAttributeName = TpsVisualCueContracts.StressAttributeName,
                    stressValue = TpsVisualCueContracts.StressAttributeValue,
                    volumeAttributeName = TpsVisualCueContracts.VolumeAttributeName
                },
                new() { Timeout = BrowserTestConstants.Timing.EditorMutationTimeoutMs });
            var probe = await probeHandle.JsonValueAsync<EditorCueProbe>();

            await Assert.That(probe.LoudVolume).IsEqualTo(TpsVisualCueContracts.VolumeLoud);
            await Assert.That(probe.SoftVolume).IsEqualTo(TpsVisualCueContracts.VolumeSoft);
            await Assert.That(probe.BuildingDelivery).IsEqualTo(TpsVisualCueContracts.DeliveryModeBuilding);
            await Assert.That(probe.StressValue).IsEqualTo(TpsVisualCueContracts.StressAttributeValue);
            await Assert.That(probe.LegatoArticulation).IsEqualTo(TpsVisualCueContracts.ArticulationLegato);
            await Assert.That(probe.StaccatoArticulation).IsEqualTo(TpsVisualCueContracts.ArticulationStaccato);
            await Assert.That(probe.EnergyValue).IsEqualTo("8");
            await Assert.That(probe.MelodyValue).IsEqualTo("4");
            await Assert.That(probe.BreathValue).IsEqualTo(TpsVisualCueContracts.BreathAttributeValue);
            await Assert.That(string.IsNullOrWhiteSpace(probe.LoudScale)).IsFalse();
            await Assert.That(string.IsNullOrWhiteSpace(probe.SoftScale)).IsFalse();

            await UiScenarioArtifacts.CapturePageAsync(page, CueScenario, OverlayStepName);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Test]
    public async Task EditorScreen_RendersMonacoCueStylesImmediatelyAfterImport()
    {
        UiScenarioArtifacts.ResetScenario(CueScenario);

        var page = await fixture.NewPageAsync(additionalContext: true);

        try
        {
            await EditorIsolatedDraftDriver.OpenBlankDraftAsync(page);
            await EditorMonacoDriver.SetTextAsync(
                page,
                """
                ## [Cue Import|140WPM|Professional]
                ### [Delivery Block|140WPM|Warm]
                [loud][building]Rise together[/building][/loud] and [soft][emphasis]listen closely[/emphasis][/soft]. //
                [pronunciation:TELE-promp-ter]teleprompter[/pronunciation] [highlight]tonight[/highlight]
                [legato][energy:8]steady[/energy][/legato] [staccato][melody:4]rhythm[/melody][/staccato]
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
                        classes.some(value => value.includes(args.legatoClass)) &&
                        classes.some(value => value.includes(args.energyClass)) &&
                        classes.some(value => value.includes(args.staccatoClass)) &&
                        classes.some(value => value.includes(args.melodyClass)) &&
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
                    energyClass = "po-inline-energy",
                    legatoClass = "po-inline-articulation-legato",
                    loudClass = "po-inline-loud",
                    melodyClass = "po-inline-melody",
                    pauseClass = "po-pause-long",
                    pronunciationClass = "po-inline-pronunciation-word",
                    staccatoClass = "po-inline-articulation-staccato",
                    stageTestId = UiTestIds.Editor.SourceStage
                },
                new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });

            var state = await EditorMonacoDriver.GetStateAsync(page);
            await Assert.That(state.Text).Contains("## [Cue Import|140WPM|Professional]");
            await Assert.That(HasDecorationToken(state, "po-inline-emphasis")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-inline-highlight")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-inline-loud")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-inline-articulation-legato")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-inline-energy")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-inline-articulation-staccato")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-inline-melody")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-pause-long")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-inline-pronunciation-word")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-tag")).IsTrue();
            await Assert.That(HasDecorationToken(state, "po-header-emotion")).IsTrue();

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

        public string LegatoArticulation { get; init; } = string.Empty;

        public string StaccatoArticulation { get; init; } = string.Empty;

        public string EnergyValue { get; init; } = string.Empty;

        public string MelodyValue { get; init; } = string.Empty;

        public string BreathValue { get; init; } = string.Empty;

        public string LoudScale { get; init; } = string.Empty;

        public string SoftScale { get; init; } = string.Empty;
    }

    private static bool HasDecorationToken(EditorMonacoState state, string decorationToken) =>
        state.DecorationClasses.Any(value => value.Contains(decorationToken, StringComparison.Ordinal));
}
