using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Pages;
using PrompterOne.Shared.Tests;

namespace PrompterOne.Web.Tests;

public sealed class TeleprompterCueRenderingTests : BunitContext
{
    private const int CueCardIndex = 0;
    private const string BuildingFirstWord = "we";
    private const string BuildingLastWord = "together.";
    private const string CueScriptId = "test-reader-cue-script";
    private const string CueScriptTitle = "Reader Cue Probe";
    private const string EnergyWord = "steady";
    private const string LoudWord = "clear";
    private const string MelodyWord = "rhythm";
    private const string SoftWord = "gentle";
    private const string StressWord = "building.";
    private const string WhisperWord = "secret";

    [Test]
    public async Task TeleprompterPage_EmitsCueAttributesAndScaleVariablesForReaderDeliverySemantics()
    {
        var harness = TestHarnessFactory.Create(this, seedLibraryData: false);
        await harness.Repository.SaveAsync(
                CueScriptTitle,
                """
                ---
                title: "Reader Cue Probe"
                base_wpm: 140
                ---

                ## [Cue Demo|neutral]

                ### [Delivery Block|neutral]
                [whisper]secret[/whisper] [soft]gentle[/soft] [loud]clear[/loud] build[stress]ing[/stress]. //
                [breath]
                [legato][energy:8]steady[/energy][/legato] [staccato][melody:4]rhythm[/melody][/staccato] //

                ### [Lift Block|neutral]
                [building]we rise together[/building].
                """,
                "reader-cue-probe.tps",
                CueScriptId);

        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppRoutes.TeleprompterWithId(CueScriptId));
        var cut = Render<TeleprompterPage>();

        cut.WaitForAssertion(() =>
        {
            var whisper = FindReaderWordByText(cut, CueCardIndex, WhisperWord);
            var soft = FindReaderWordByText(cut, CueCardIndex, SoftWord);
            var loud = FindReaderWordByText(cut, CueCardIndex, LoudWord);
            var stress = FindReaderWordByText(cut, CueCardIndex, StressWord);
            var energy = FindReaderWordByText(cut, CueCardIndex, EnergyWord);
            var melody = FindReaderWordByText(cut, CueCardIndex, MelodyWord);
            var breath = FindBreathCue(cut, CueCardIndex);
            var buildingFirst = FindReaderWordByText(cut, 1, BuildingFirstWord);
            var buildingLast = FindReaderWordByText(cut, 1, BuildingLastWord);

            Assert.Equal(TpsVisualCueContracts.VolumeWhisper, whisper.GetAttribute(TpsVisualCueContracts.VolumeAttributeName));
            Assert.Equal(TpsVisualCueContracts.VolumeSoft, soft.GetAttribute(TpsVisualCueContracts.VolumeAttributeName));
            Assert.Equal(TpsVisualCueContracts.VolumeLoud, loud.GetAttribute(TpsVisualCueContracts.VolumeAttributeName));
            Assert.Equal(TpsVisualCueContracts.DeliveryModeBuilding, buildingFirst.GetAttribute(TpsVisualCueContracts.DeliveryAttributeName));
            Assert.Equal(TpsVisualCueContracts.DeliveryModeBuilding, buildingLast.GetAttribute(TpsVisualCueContracts.DeliveryAttributeName));
            Assert.Equal(TpsVisualCueContracts.StressAttributeValue, stress.GetAttribute(TpsVisualCueContracts.StressAttributeName));
            Assert.Equal(TpsVisualCueContracts.ArticulationLegato, energy.GetAttribute(TpsVisualCueContracts.ArticulationAttributeName));
            Assert.Equal("8", energy.GetAttribute(TpsVisualCueContracts.EnergyAttributeName));
            Assert.Equal(TpsVisualCueContracts.ArticulationStaccato, melody.GetAttribute(TpsVisualCueContracts.ArticulationAttributeName));
            Assert.Equal("4", melody.GetAttribute(TpsVisualCueContracts.MelodyAttributeName));
            Assert.Equal(TpsVisualCueContracts.BreathAttributeValue, breath.GetAttribute(TpsVisualCueContracts.BreathAttributeName));

            var whisperScale = ReadStyleVariable(whisper, TpsVisualCueContracts.CueScaleVariableName);
            var softScale = ReadStyleVariable(soft, TpsVisualCueContracts.CueScaleVariableName);
            var loudScale = ReadStyleVariable(loud, TpsVisualCueContracts.CueScaleVariableName);
            var stressScale = ReadStyleVariable(stress, TpsVisualCueContracts.CueScaleVariableName);
            var energyScale = ReadStyleVariable(energy, TpsVisualCueContracts.CueScaleVariableName);
            var melodyScale = ReadStyleVariable(melody, TpsVisualCueContracts.CueScaleVariableName);
            var energyLevel = ReadStyleVariable(energy, TpsVisualCueContracts.EnergyVariableName);
            var melodyLevel = ReadStyleVariable(melody, TpsVisualCueContracts.MelodyVariableName);
            var buildingFirstScale = ReadStyleVariable(buildingFirst, TpsVisualCueContracts.CueScaleVariableName);
            var buildingLastScale = ReadStyleVariable(buildingLast, TpsVisualCueContracts.CueScaleVariableName);

            Assert.True(whisperScale < softScale, $"Expected whisper scale < soft scale, got {whisperScale} and {softScale}.");
            Assert.True(softScale < 1d, $"Expected soft scale below 1, got {softScale}.");
            Assert.True(loudScale > 1d, $"Expected loud scale above 1, got {loudScale}.");
            Assert.True(stressScale > 1d, $"Expected stress scale above 1, got {stressScale}.");
            Assert.True(energyScale > 1d, $"Expected energy scale above 1, got {energyScale}.");
            Assert.True(melodyScale > 1d, $"Expected melody scale above 1, got {melodyScale}.");
            Assert.Equal(0.8d, energyLevel);
            Assert.Equal(0.4d, melodyLevel);
            Assert.True(buildingLastScale > buildingFirstScale, $"Expected building ramp to increase, got {buildingFirstScale} then {buildingLastScale}.");
        });
    }

    private static AngleSharp.Dom.IElement FindReaderWordByText(IRenderedComponent<TeleprompterPage> cut, int cardIndex, string text) =>
        cut.FindByTestId(UiTestIds.Teleprompter.CardText(cardIndex))
            .QuerySelectorAll(BunitTestSelectors.BuildTestIdPrefixSelector(UiTestIds.Teleprompter.CardWordPrefix(cardIndex)))
            .Single(element => string.Equals(element.TextContent.Trim(), text, StringComparison.Ordinal));

    private static AngleSharp.Dom.IElement FindBreathCue(IRenderedComponent<TeleprompterPage> cut, int cardIndex) =>
        cut.FindByTestId(UiTestIds.Teleprompter.CardText(cardIndex))
            .QuerySelector($"[{TpsVisualCueContracts.BreathAttributeName}='{TpsVisualCueContracts.BreathAttributeValue}']")
        ?? throw new InvalidOperationException("Expected reader breath cue to render.");

    private static double ReadStyleVariable(AngleSharp.Dom.IElement element, string variableName)
    {
        var style = element.GetAttribute("style") ?? string.Empty;
        var segments = style.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var segment in segments)
        {
            var parts = segment.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && string.Equals(parts[0], variableName, StringComparison.Ordinal))
            {
                return double.Parse(parts[1], CultureInfo.InvariantCulture);
            }
        }

        return 1d;
    }
}
