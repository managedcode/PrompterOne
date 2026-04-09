using PrompterOne.Core.Models.Editor;
using PrompterOne.Core.Services.Editor;

namespace PrompterOne.Core.Tests;

public sealed class TpsDocumentSplitServiceTests
{
    private readonly TpsDocumentSplitService _service = new();
    private readonly TpsFrontMatterDocumentService _frontMatter = new();

    [Test]
    public void Split_BySegmentHeading_CreatesOneChildPerEpisodeAndCarriesCanonicalMetadata()
    {
        var source =
            """
            ---
            title: "System Design and Software Architecture for Vibe Coders"
            profile: "Actor"
            duration: "145:00"
            base_wpm: 140
            author: "Konstantin Semenenko"
            created: "2026-03-25"
            version: "1.0"
            ---

            ## [Episode 1 - How to Think About Systems|140WPM|Professional]
            Before you write code, / you need to think about the system. //

            ## [Episode 2 - How Systems Talk to Each Other|140WPM|Professional]
            APIs, events, and retries matter. //
            """;

        var result = _service.Split(source, TpsDocumentSplitMode.SegmentHeading);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal(1, first.Sequence);
                Assert.Equal("Episode 1 - How to Think About Systems", first.Title);
                var document = _frontMatter.Parse(first.Text);
                Assert.Equal(first.Title, document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Title]);
                Assert.Equal("Actor", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Profile]);
                Assert.Equal("140", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.BaseWpm]);
                Assert.Equal("Konstantin Semenenko", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Author]);
                Assert.Equal("2026-03-25", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Created]);
                Assert.Equal("1.0", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Version]);
                Assert.DoesNotContain(TpsFrontMatterDocumentService.MetadataKeys.Duration, document.Metadata.Keys, StringComparer.OrdinalIgnoreCase);
                Assert.StartsWith("## [Episode 1 - How to Think About Systems|140WPM|Professional]", document.Body, StringComparison.Ordinal);
            },
            second =>
            {
                Assert.Equal(2, second.Sequence);
                Assert.Equal("Episode 2 - How Systems Talk to Each Other", second.Title);
                var document = _frontMatter.Parse(second.Text);
                Assert.Equal(second.Title, document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Title]);
                Assert.StartsWith("## [Episode 2 - How Systems Talk to Each Other|140WPM|Professional]", document.Body, StringComparison.Ordinal);
            });
    }

    [Test]
    public void Split_ByTopLevelHeading_KeepsPreambleWithFirstSection()
    {
        var source =
            """
            ---
            title: "Architecture Course"
            profile: "Actor"
            base_wpm: 150
            ---

            Opening note before the first section.

            # Episode 1
            ## [Intro|150WPM|Focused]
            Think in boundaries.

            # Episode 2
            ## [Queues|150WPM|Focused]
            Think in messages.
            """;

        var result = _service.Split(source, TpsDocumentSplitMode.TopLevelHeading);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("Episode 1", first.Title);
                var document = _frontMatter.Parse(first.Text);
                Assert.Contains("Opening note before the first section.", document.Body, StringComparison.Ordinal);
                Assert.Contains("# Episode 1", document.Body, StringComparison.Ordinal);
                Assert.DoesNotContain("# Episode 2", document.Body, StringComparison.Ordinal);
            },
            second =>
            {
                Assert.Equal("Episode 2", second.Title);
                var document = _frontMatter.Parse(second.Text);
                Assert.StartsWith("# Episode 2", document.Body, StringComparison.Ordinal);
            });
    }

    [Test]
    public void Split_BySpeaker_CreatesOneChildPerSpeakerAndKeepsOnlyThatSpeakerContent()
    {
        var source =
            """
            ---
            title: "Interview Day"
            profile: "Actor"
            duration: "12:00"
            author: "Konstantin Semenenko"
            ---

            ## [Opening|Speaker:Alex|140WPM|warm]
            Alex intro line. //

            ### [Question|Speaker:Jordan|130WPM|focused]
            Jordan asks the question. //

            ### [Answer|135WPM|professional]
            Alex answers with inherited speaker context. //

            ## [Wrap Up|150WPM|motivational]
            ### [Signoff|Speaker:Jordan|150WPM|focused]
            Jordan closes the interview. //
            """;

        var result = _service.Split(source, TpsDocumentSplitMode.Speaker);

        Assert.Collection(
            result,
            alex =>
            {
                Assert.Equal(1, alex.Sequence);
                Assert.Equal("Alex", alex.Title);
                var document = _frontMatter.Parse(alex.Text);
                Assert.Equal("Alex", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Title]);
                Assert.Equal("Actor", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Profile]);
                Assert.Equal("Konstantin Semenenko", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Author]);
                Assert.DoesNotContain(TpsFrontMatterDocumentService.MetadataKeys.Duration, document.Metadata.Keys, StringComparer.OrdinalIgnoreCase);
                Assert.Contains("## [Opening|Speaker:Alex|140WPM|warm]", document.Body, StringComparison.Ordinal);
                Assert.Contains("Alex intro line. //", document.Body, StringComparison.Ordinal);
                Assert.Contains("### [Answer|135WPM|professional]", document.Body, StringComparison.Ordinal);
                Assert.Contains("Alex answers with inherited speaker context. //", document.Body, StringComparison.Ordinal);
                Assert.DoesNotContain("Jordan asks the question. //", document.Body, StringComparison.Ordinal);
                Assert.DoesNotContain("Jordan closes the interview. //", document.Body, StringComparison.Ordinal);
            },
            jordan =>
            {
                Assert.Equal(2, jordan.Sequence);
                Assert.Equal("Jordan", jordan.Title);
                var document = _frontMatter.Parse(jordan.Text);
                Assert.Equal("Jordan", document.Metadata[TpsFrontMatterDocumentService.MetadataKeys.Title]);
                Assert.Contains("## [Opening|140WPM|warm]", document.Body, StringComparison.Ordinal);
                Assert.DoesNotContain("## [Opening|Speaker:Alex|140WPM|warm]", document.Body, StringComparison.Ordinal);
                Assert.Contains("### [Question|Speaker:Jordan|130WPM|focused]", document.Body, StringComparison.Ordinal);
                Assert.Contains("Jordan asks the question. //", document.Body, StringComparison.Ordinal);
                Assert.Contains("## [Wrap Up|150WPM|motivational]", document.Body, StringComparison.Ordinal);
                Assert.Contains("### [Signoff|Speaker:Jordan|150WPM|focused]", document.Body, StringComparison.Ordinal);
                Assert.Contains("Jordan closes the interview. //", document.Body, StringComparison.Ordinal);
                Assert.DoesNotContain("Alex intro line. //", document.Body, StringComparison.Ordinal);
                Assert.DoesNotContain("Alex answers with inherited speaker context. //", document.Body, StringComparison.Ordinal);
            });
    }

    [Test]
    public void Split_ReturnsEmptyWhenBodyDoesNotContainRequestedHeadingLevel()
    {
        var source =
            """
            ---
            title: "Single Script"
            profile: "Actor"
            ---

            Plain text without split headings.
            """;

        var result = _service.Split(source, TpsDocumentSplitMode.SegmentHeading);

        Assert.Empty(result);
    }
}
