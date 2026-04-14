using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Rendering;

namespace PrompterOne.Web.Tests;

public sealed class EditorCueRenderingTests
{
    private const string BreathAttribute = TpsVisualCueContracts.BreathAttributeName + "=\"" + TpsVisualCueContracts.BreathAttributeValue + "\"";
    private const string BuildingAttribute = TpsVisualCueContracts.DeliveryAttributeName + "=\"" + TpsVisualCueContracts.DeliveryModeBuilding + "\"";
    private const string EnergyAttribute = TpsVisualCueContracts.EnergyAttributeName + "=\"8\"";
    private const string HighlightAttribute = TpsVisualCueContracts.HighlightAttributeName + "=\"" + TpsVisualCueContracts.HighlightAttributeValue + "\"";
    private const string LegatoAttribute = TpsVisualCueContracts.ArticulationAttributeName + "=\"" + TpsVisualCueContracts.ArticulationLegato + "\"";
    private const string LoudAttribute = TpsVisualCueContracts.VolumeAttributeName + "=\"" + TpsVisualCueContracts.VolumeLoud + "\"";
    private const string MelodyAttribute = TpsVisualCueContracts.MelodyAttributeName + "=\"4\"";
    private const string SoftAttribute = TpsVisualCueContracts.VolumeAttributeName + "=\"" + TpsVisualCueContracts.VolumeSoft + "\"";
    private const string StaccatoAttribute = TpsVisualCueContracts.ArticulationAttributeName + "=\"" + TpsVisualCueContracts.ArticulationStaccato + "\"";
    private const string StressAttribute = TpsVisualCueContracts.StressAttributeName + "=\"" + TpsVisualCueContracts.StressAttributeValue + "\"";
    private const string XslowAttribute = TpsVisualCueContracts.SpeedAttributeName + "=\"" + TpsVisualCueContracts.SpeedCueXslow + "\"";

    [Test]
    public void Render_EmitsStableCueAttributesForVolumeDeliveryStressAndSpeed()
    {
        var markup = EditorMarkupRenderer.Render(
            """
            [loud][building]Rise carefully[/building][/loud] and [soft][xslow]listen[stress]ing[/stress][/xslow][/soft].
            [breath] [legato][energy:8]steady[/energy][/legato] [staccato][melody:4][highlight]rhythm[/highlight][/melody][/staccato].
            """).Value;

        Assert.Contains(LoudAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(BuildingAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(SoftAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(StressAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(XslowAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(BreathAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(LegatoAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(EnergyAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(StaccatoAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(MelodyAttribute, markup, StringComparison.Ordinal);
        Assert.Contains(HighlightAttribute, markup, StringComparison.Ordinal);
    }
}
