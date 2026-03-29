using PrompterLive.Core.Services;
using PrompterLive.Core.Services.Rsvp;

namespace PrompterLive.Core.Tests;

public sealed class TpsRoundTripTests
{
    [Fact]
    public async Task ParseAndExportAsync_RetainsMetadataAndSegmentShape()
    {
        var parser = new TpsParser();
        var exporter = new TpsExporter();
        const string source = """
        ---
        title: "File Export Test"
        base_wpm: 150
        ---

        ## [Test Segment|150WPM|warm]

        ### [Body|150WPM]

        Content to export.
        """;

        var document = await parser.ParseAsync(source);
        var exported = await exporter.ExportAsync(document);
        var reparsed = await parser.ParseAsync(exported);

        Assert.Equal("File Export Test", reparsed.Metadata["title"]);
        Assert.Equal("150", reparsed.Metadata["base_wpm"]);
        Assert.Single(reparsed.Segments);
        Assert.Equal("Test Segment", reparsed.Segments[0].Name);
        Assert.Contains(reparsed.Segments[0].Blocks, block => block.Name == "Body");
    }

    [Fact]
    public void OrpCalculator_SplitsLongWordAtExpectedFocusPoint()
    {
        var calculator = new RsvpOrpCalculator();

        var split = calculator.SplitWordAtORP("teleprompter");

        Assert.Equal("tele", split.PreORP);
        Assert.Equal("p", split.OrpChar);
        Assert.Equal("rompter", split.PostORP);
    }
}
