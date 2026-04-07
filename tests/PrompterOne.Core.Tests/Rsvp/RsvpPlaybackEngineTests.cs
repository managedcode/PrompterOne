using PrompterOne.Core.Services.Rsvp;

namespace PrompterOne.Core.Tests;

public sealed class RsvpPlaybackEngineTests
{
    [Test]
    [Arguments("Done.\"")]
    [Arguments("Done.”")]
    [Arguments("Done?'")]
    [Arguments("Done?’")]
    public void GetWordDisplayTime_TreatsTrailingQuotesAfterSentenceEndingPunctuationAsSentenceEndings(string word)
    {
        var engine = new RsvpPlaybackEngine
        {
            WordsPerMinute = 120
        };

        var plainDuration = engine.GetWordDisplayTime("Done");
        var sentenceDuration = engine.GetWordDisplayTime("Done.");
        var quotedDuration = engine.GetWordDisplayTime(word);

        Assert.Equal(sentenceDuration, quotedDuration);
        Assert.True(quotedDuration > plainDuration);
    }
}
