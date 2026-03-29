using PrompterLive.Core.Services.Rsvp;

namespace PrompterLive.Core.Tests;

public sealed class RsvpEmotionAnalyzerTests
{
    [Fact]
    public void AnalyzeWord_ReturnsExpectedSemanticEmotion()
    {
        var analyzer = new RsvpEmotionAnalyzer();

        Assert.Equal("happy", analyzer.AnalyzeWord("amazing"));
        Assert.Equal("professional", analyzer.AnalyzeWord("performance"));
        Assert.Equal("peaceful", analyzer.AnalyzeWord("serenity"));
    }

    [Fact]
    public void UpdateEmotionForWord_TransitionsAndResetsToDefault()
    {
        var analyzer = new RsvpEmotionAnalyzer();

        var changedToExcited = analyzer.UpdateEmotionForWord("awesome");
        Assert.True(changedToExcited);
        Assert.Equal("Excited", analyzer.CurrentEmotion.Name);

        var changedBackToDefault = analyzer.UpdateEmotionForWord("ordinary");

        Assert.True(changedBackToDefault);
        Assert.Equal("Neutral", analyzer.CurrentEmotion.Name);
    }

    [Fact]
    public void SetEmotion_IgnoresUnknownKeys()
    {
        var analyzer = new RsvpEmotionAnalyzer();

        analyzer.SetEmotion("calm");
        analyzer.SetEmotion("missing-emotion");

        Assert.Equal("Calm", analyzer.CurrentEmotion.Name);
        Assert.Equal("#4ECDC4", analyzer.CurrentEmotion.ColorHex);
    }
}
