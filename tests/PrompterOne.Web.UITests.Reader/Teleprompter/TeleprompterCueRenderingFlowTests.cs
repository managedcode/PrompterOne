using System.Globalization;
using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

[ClassDataSource<StandaloneAppFixture>(Shared = SharedType.PerClass)]
public sealed class TeleprompterCueRenderingFlowTests(StandaloneAppFixture fixture)
{
    private const string CueScenario = "teleprompter-tps-cue-rendering";
    private const int InspirationCardIndex = 6;
    private const string StepName = "01-teleprompter-cue-rendering";
    private const string CueTextStepName = "02-teleprompter-cue-text";

    [Test]
    public async Task TeleprompterDemo_RendersTypographyDrivenCueVariablesForVolumeAndDeliveryTexture()
    {
        UiScenarioArtifacts.ResetScenario(CueScenario);

        var page = await fixture.NewPageAsync(additionalContext: true);

        try
        {
            await ReaderRouteDriver.OpenTeleprompterAsync(page, BrowserTestConstants.Routes.TeleprompterDemo);
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            var cardText = page.GetByTestId(UiTestIds.Teleprompter.CardText(InspirationCardIndex));
            var probe = await cardText.EvaluateAsync<TeleprompterCueProbe>(
                $$"""
                host => {
                    const nodes = [...host.querySelectorAll('*')];
                    const soft = nodes.find(node =>
                        node?.getAttribute('{{TpsVisualCueContracts.VolumeAttributeName}}') === '{{TpsVisualCueContracts.VolumeSoft}}');
                    const loud = nodes.find(node =>
                        node?.getAttribute('{{TpsVisualCueContracts.VolumeAttributeName}}') === '{{TpsVisualCueContracts.VolumeLoud}}');
                    const buildingWords = nodes.filter(node =>
                        node?.getAttribute('{{TpsVisualCueContracts.DeliveryAttributeName}}') === '{{TpsVisualCueContracts.DeliveryModeBuilding}}');
                    const legato = nodes.find(node =>
                        node?.getAttribute('{{TpsVisualCueContracts.ArticulationAttributeName}}') === '{{TpsVisualCueContracts.ArticulationLegato}}');
                    const staccato = nodes.find(node =>
                        node?.getAttribute('{{TpsVisualCueContracts.ArticulationAttributeName}}') === '{{TpsVisualCueContracts.ArticulationStaccato}}');
                    const energy = nodes.find(node =>
                        node?.getAttribute('{{TpsVisualCueContracts.EnergyAttributeName}}') === '8');
                    const melody = nodes.find(node =>
                        node?.getAttribute('{{TpsVisualCueContracts.MelodyAttributeName}}') === '4');
                    const breath = nodes.find(node =>
                        node?.getAttribute('{{TpsVisualCueContracts.BreathAttributeName}}') === '{{TpsVisualCueContracts.BreathAttributeValue}}');

                    const readScale = element => {
                        if (!(element instanceof HTMLElement)) {
                            return '';
                        }

                        return getComputedStyle(element).getPropertyValue('{{TpsVisualCueContracts.CueScaleVariableName}}').trim();
                    };
                    const readVariable = (element, name) => {
                        if (!(element instanceof HTMLElement)) {
                            return '';
                        }

                        return getComputedStyle(element).getPropertyValue(name).trim();
                    };

                    return {
                        softVolume: soft?.getAttribute('{{TpsVisualCueContracts.VolumeAttributeName}}') ?? '',
                        loudVolume: loud?.getAttribute('{{TpsVisualCueContracts.VolumeAttributeName}}') ?? '',
                        firstBuildingDelivery: buildingWords[0]?.getAttribute('{{TpsVisualCueContracts.DeliveryAttributeName}}') ?? '',
                        lastBuildingDelivery: buildingWords.at(-1)?.getAttribute('{{TpsVisualCueContracts.DeliveryAttributeName}}') ?? '',
                        legatoArticulation: legato?.getAttribute('{{TpsVisualCueContracts.ArticulationAttributeName}}') ?? '',
                        staccatoArticulation: staccato?.getAttribute('{{TpsVisualCueContracts.ArticulationAttributeName}}') ?? '',
                        energyValue: energy?.getAttribute('{{TpsVisualCueContracts.EnergyAttributeName}}') ?? '',
                        melodyValue: melody?.getAttribute('{{TpsVisualCueContracts.MelodyAttributeName}}') ?? '',
                        breathValue: breath?.getAttribute('{{TpsVisualCueContracts.BreathAttributeName}}') ?? '',
                        softScale: readScale(soft),
                        loudScale: readScale(loud),
                        firstBuildingScale: readScale(buildingWords[0]),
                        lastBuildingScale: readScale(buildingWords.at(-1)),
                        energyScale: readScale(energy),
                        melodyScale: readScale(melody),
                        energyTone: readVariable(energy, '{{TpsVisualCueContracts.EnergyVariableName}}'),
                        melodyTone: readVariable(melody, '{{TpsVisualCueContracts.MelodyVariableName}}')
                    };
                }
                """);

            await Assert.That(probe.SoftVolume).IsEqualTo(TpsVisualCueContracts.VolumeSoft);
            await Assert.That(probe.LoudVolume).IsEqualTo(TpsVisualCueContracts.VolumeLoud);
            await Assert.That(probe.FirstBuildingDelivery).IsEqualTo(TpsVisualCueContracts.DeliveryModeBuilding);
            await Assert.That(probe.LastBuildingDelivery).IsEqualTo(TpsVisualCueContracts.DeliveryModeBuilding);
            await Assert.That(probe.LegatoArticulation).IsEqualTo(TpsVisualCueContracts.ArticulationLegato);
            await Assert.That(probe.StaccatoArticulation).IsEqualTo(TpsVisualCueContracts.ArticulationStaccato);
            await Assert.That(probe.EnergyValue).IsEqualTo("8");
            await Assert.That(probe.MelodyValue).IsEqualTo("4");
            await Assert.That(probe.BreathValue).IsEqualTo(TpsVisualCueContracts.BreathAttributeValue);
            await Assert.That(string.IsNullOrWhiteSpace(probe.SoftScale)).IsFalse();
            await Assert.That(string.IsNullOrWhiteSpace(probe.LoudScale)).IsFalse();
            await Assert.That(string.IsNullOrWhiteSpace(probe.FirstBuildingScale)).IsFalse();
            await Assert.That(string.IsNullOrWhiteSpace(probe.LastBuildingScale)).IsFalse();
            await Assert.That(double.Parse(probe.EnergyScale, CultureInfo.InvariantCulture)).IsGreaterThan(1d);
            await Assert.That(double.Parse(probe.MelodyScale, CultureInfo.InvariantCulture)).IsGreaterThan(1d);
            await Assert.That(double.Parse(probe.EnergyTone, CultureInfo.InvariantCulture)).IsEqualTo(0.8d);
            await Assert.That(double.Parse(probe.MelodyTone, CultureInfo.InvariantCulture)).IsEqualTo(0.4d);

            for (var index = 0; index < InspirationCardIndex; index++)
            {
                await page.GetByTestId(UiTestIds.Teleprompter.NextBlock).ClickAsync();
            }

            await Expect(cardText).ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
            var activeWord = page.GetByTestId(UiTestIds.Teleprompter.ActiveWord);
            for (var index = 0; index < 60; index++)
            {
                var text = await activeWord.TextContentAsync(new() { Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs });
                if (text?.Contains("steady", StringComparison.OrdinalIgnoreCase) == true)
                {
                    break;
                }

                await page.GetByTestId(UiTestIds.Teleprompter.NextWord).ClickAsync();
            }

            await Expect(activeWord).ToContainTextAsync("steady", new()
            {
                Timeout = BrowserTestConstants.Timing.DefaultVisibleTimeoutMs
            });
            await UiScenarioArtifacts.CapturePageAsync(page, CueScenario, StepName);
            await UiScenarioArtifacts.CaptureLocatorAsync(cardText, CueScenario, CueTextStepName);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private sealed class TeleprompterCueProbe
    {
        public string SoftVolume { get; init; } = string.Empty;

        public string LoudVolume { get; init; } = string.Empty;

        public string FirstBuildingDelivery { get; init; } = string.Empty;

        public string LastBuildingDelivery { get; init; } = string.Empty;

        public string LegatoArticulation { get; init; } = string.Empty;

        public string StaccatoArticulation { get; init; } = string.Empty;

        public string EnergyValue { get; init; } = string.Empty;

        public string MelodyValue { get; init; } = string.Empty;

        public string BreathValue { get; init; } = string.Empty;

        public string SoftScale { get; init; } = string.Empty;

        public string LoudScale { get; init; } = string.Empty;

        public string FirstBuildingScale { get; init; } = string.Empty;

        public string LastBuildingScale { get; init; } = string.Empty;

        public string EnergyScale { get; init; } = string.Empty;

        public string MelodyScale { get; init; } = string.Empty;

        public string EnergyTone { get; init; } = string.Empty;

        public string MelodyTone { get; init; } = string.Empty;
    }
}
