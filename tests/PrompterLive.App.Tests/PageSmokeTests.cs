using Bunit;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class PageSmokeTests : BunitContext
{
    public PageSmokeTests()
    {
        TestHarnessFactory.Create(this);
    }

    [Fact]
    public void LibraryPage_RendersExactDesignShell()
    {
        var cut = Render<LibraryPage>();

        Assert.Contains("screen-library", cut.Markup);
        Assert.Contains("Product Launch", cut.Markup);
        Assert.Contains("Quantum Computing", cut.Markup);
        Assert.Contains("New Script", cut.Markup);
    }

    [Fact]
    public void EditorPage_RendersToolbarStructureAndMetadataRail()
    {
        var cut = Render<EditorPage>();

        Assert.Contains("STRUCTURE", cut.Markup);
        Assert.Contains("TPS Emotions", cut.Markup);
        Assert.Contains("Call to Action", cut.Markup);
        Assert.Contains("METADATA", cut.Markup);
    }

    [Fact]
    public void LearnPage_RendersRsvpSurface()
    {
        var cut = Render<LearnPage>();

        Assert.Contains("screen-rsvp", cut.Markup);
        Assert.Contains("rsvp-speed", cut.Markup);
        Assert.Contains("rsvp-progress-label", cut.Markup);
    }

    [Fact]
    public void TeleprompterPage_RendersReaderControls()
    {
        var cut = Render<TeleprompterPage>();

        Assert.Contains("screen-teleprompter", cut.Markup);
        Assert.Contains("rd-countdown", cut.Markup);
        Assert.Contains("rd-block-indicator", cut.Markup);
    }

    [Fact]
    public void SettingsPage_RendersSectionPanels()
    {
        var cut = Render<SettingsPage>();

        Assert.Contains("Cloud Storage", cut.Markup);
        Assert.Contains("File Storage", cut.Markup);
        Assert.Contains("Streaming", cut.Markup);
        Assert.Contains("AI Provider", cut.Markup);
    }
}
