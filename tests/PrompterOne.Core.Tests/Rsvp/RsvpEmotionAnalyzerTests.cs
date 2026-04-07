using ManagedCode.Tps;
using PrompterOne.Core.Models.HeadCues;
using PrompterOne.Core.Services.Rsvp;

namespace PrompterOne.Core.Tests;

public sealed class RsvpEmotionAnalyzerTests
{
    [Test]
    public void AnalyzeWord_ReturnsExpectedTpsEmotionKeys()
    {
        var analyzer = new RsvpEmotionAnalyzer();

        Assert.Equal(TpsSpec.EmotionNames.Happy, analyzer.AnalyzeWord("amazing"));
        Assert.Equal(TpsSpec.EmotionNames.Professional, analyzer.AnalyzeWord("performance"));
        Assert.Equal(TpsSpec.EmotionNames.Calm, analyzer.AnalyzeWord("serenity"));
    }

    [Test]
    public void UpdateEmotionForWord_TransitionsAndResetsToDefaultEmotion()
    {
        var analyzer = new RsvpEmotionAnalyzer();

        var changedToExcited = analyzer.UpdateEmotionForWord("awesome");
        Assert.True(changedToExcited);
        Assert.Equal(TpsSpec.EmotionNames.Excited, analyzer.CurrentEmotionKey);

        var changedBackToDefault = analyzer.UpdateEmotionForWord("ordinary");

        Assert.True(changedBackToDefault);
        Assert.Equal(TpsSpec.DefaultEmotion, analyzer.CurrentEmotionKey);
    }

    [Test]
    public void SetEmotion_IgnoresUnknownKeys()
    {
        var analyzer = new RsvpEmotionAnalyzer();

        analyzer.SetEmotion(TpsSpec.EmotionNames.Calm);
        analyzer.SetEmotion("missing-emotion");

        Assert.Equal(TpsSpec.EmotionNames.Calm, analyzer.CurrentEmotionKey);
    }

    [Test]
    public void HeadCueCatalog_ResolvesEmotionHeadCuesFromTpsSpec()
    {
        Assert.Equal(TpsSpec.EmotionHeadCues[TpsSpec.DefaultEmotion], HeadCueCatalog.DefaultCueId);
        Assert.Equal(TpsSpec.EmotionHeadCues[TpsSpec.EmotionNames.Happy], HeadCueCatalog.ResolveForEmotion(TpsSpec.EmotionNames.Happy));
        Assert.Equal(TpsSpec.EmotionHeadCues[TpsSpec.DefaultEmotion], HeadCueCatalog.ResolveForEmotion("missing-emotion"));
    }
}
