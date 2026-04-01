using PrompterOne.Core.Models.Editor;
using PrompterOne.Core.Services.Editor;

namespace PrompterOne.Core.Tests;

public sealed class TpsStructureEditorTests
{
    private readonly TpsStructureEditor _editor = new();

    [Fact]
    public void TryRead_ReadsSegmentHeaderFields()
    {
        const string source = """
                              ## [Introduction|280WPM|neutral|0:00-0:00]
                              ### [Overview Block|280WPM|neutral]
                              Copy.
                              """;

        var success = _editor.TryRead(source, 0, out var snapshot);

        Assert.True(success);
        Assert.Equal(TpsStructureHeaderKind.Segment, snapshot.Kind);
        Assert.Equal("Introduction", snapshot.Name);
        Assert.Equal(280, snapshot.TargetWpm);
        Assert.Equal("neutral", snapshot.EmotionKey);
        Assert.Equal("0:00-0:00", snapshot.Timing);
    }

    [Fact]
    public void Update_RewritesBlockHeaderSafely()
    {
        const string source = """
                              ## [Introduction|280WPM|neutral|0:00-0:00]
                              ### [Overview Block|280WPM|neutral]
                              Copy.
                              """;

        var startIndex = source.IndexOf("### [Overview Block|280WPM|neutral]", StringComparison.Ordinal);
        var success = _editor.TryRead(source, startIndex, out var snapshot);

        Assert.True(success);

        var mutation = _editor.Update(
            source,
            snapshot with
            {
                Name = "Closing Block",
                TargetWpm = 320,
                EmotionKey = "focused"
            });

        Assert.Contains("### [Closing Block|320WPM|focused]", mutation.Text, StringComparison.Ordinal);
        Assert.Contains("Copy.", mutation.Text, StringComparison.Ordinal);
        Assert.True(mutation.Selection.Start < mutation.Selection.End);
    }
}
