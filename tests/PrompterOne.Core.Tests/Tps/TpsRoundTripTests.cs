using PrompterOne.Core.Services;
using PrompterOne.Core.Services.Rsvp;

namespace PrompterOne.Core.Tests;

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

    [Fact]
    public async Task CompileAsync_PreservesInlineWpmScopesAcrossNestedPronunciationTags()
    {
        var parser = new TpsParser();
        var compiler = new ScriptCompiler();
        const string source = """
        ---
        title: "Inline speed"
        base_wpm: 140
        ---

        ## [Call to Action|140WPM|motivational]

        ### [Closing Block|140WPM|energetic]

        [180WPM]Join us in building the future of [pronunciation:TELE-promp-ter]teleprompter[/pronunciation] technology.[/180WPM]
        """;

        var document = await parser.ParseAsync(source);
        var compiled = await compiler.CompileAsync(document);
        var compiledWord = compiled.Segments
            .SelectMany(segment => segment.Blocks)
            .SelectMany(block => block.Words)
            .Single(word => string.Equals(word.CleanText, "teleprompter", StringComparison.Ordinal));

        Assert.Equal(180, compiledWord.Metadata.SpeedOverride);
        Assert.Equal("TELE-promp-ter", compiledWord.Metadata.PronunciationGuide);
    }
}
