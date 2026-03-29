using Bunit;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class EditorSourceInteractionTests : BunitContext
{
    private readonly AppHarness _harness;

    public EditorSourceInteractionTests()
    {
        _harness = TestHarnessFactory.Create(this);
    }

    [Fact]
    public void EditorPage_UsesRawSourceTextareaAndRebuildsStructureWhenSourceChanges()
    {
        var cut = Render<EditorPage>();

        cut.WaitForAssertion(() =>
        {
            var source = cut.Find("[data-testid='editor-source-input']");
            Assert.Contains("## [Intro|140WPM|warm]", source.GetAttribute("value"));
        });

        var updatedSource =
            """
            ---
            title: "Product Launch"
            author: "PrompterLive"
            profile: "Actor"
            base_wpm: 140
            version: "1.0"
            created: "2026-03-26"
            ---

            ## [Fresh Opening|160WPM|focused]
            ### [Renamed Block|160WPM]
            Fresh copy for the editor runtime.
            """;

        cut.Find("[data-testid='editor-source-input']").Input(updatedSource);

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(updatedSource, _harness.Session.State.Text);
            Assert.Contains("Fresh Opening", cut.Markup);
            Assert.Contains("Renamed Block", cut.Markup);
            Assert.Contains("1 Segments", cut.Markup);
        });
    }

    [Fact]
    public void EditorPage_MetadataChangesRewriteRawSourceFrontMatter()
    {
        var cut = Render<EditorPage>();

        cut.WaitForAssertion(() => Assert.Contains("Product Launch", cut.Markup));

        cut.Find("[data-testid='editor-profile']").Change("RSVP");
        cut.Find("[data-testid='editor-base-wpm']").Change("210");
        cut.Find("[data-testid='editor-author']").Change("Test Speaker");
        cut.Find("[data-testid='editor-created']").Change("2026-03-26");
        cut.Find("[data-testid='editor-version']").Change("2.0");

        cut.WaitForAssertion(() =>
        {
            var source = cut.Find("[data-testid='editor-source-input']").GetAttribute("value");

            Assert.Contains("profile: \"RSVP\"", source);
            Assert.Contains("base_wpm: 210", source);
            Assert.Contains("author: \"Test Speaker\"", source);
            Assert.Contains("created: \"2026-03-26\"", source);
            Assert.Contains("version: \"2.0\"", source);
        });
    }
}
