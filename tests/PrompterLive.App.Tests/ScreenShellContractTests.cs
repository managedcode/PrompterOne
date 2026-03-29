using Bunit;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class ScreenShellContractTests : BunitContext
{
    private readonly AppHarness _harness;

    public ScreenShellContractTests()
    {
        _harness = TestHarnessFactory.Create(this);
    }

    [Fact]
    public void LibraryPage_RendersInteractiveLibraryShell()
    {
        var cut = Render<LibraryPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Find("[data-testid='library-page']"));
            Assert.NotNull(cut.Find("[data-testid='library-folder-create-tile']"));
            Assert.Contains("Product Launch", cut.Markup);
            Assert.Contains("TED: Leadership", cut.Markup);
        });
    }

    [Fact]
    public void EditorPage_RendersBodyOnlyEditorAndMetadataRail()
    {
        var cut = Render<EditorPage>();

        cut.WaitForAssertion(() =>
        {
            var sourceInput = cut.Find("[data-testid='editor-source-input']");

            Assert.DoesNotContain("profile:", sourceInput.GetAttribute("value"), StringComparison.Ordinal);
            Assert.DoesNotContain("author:", sourceInput.GetAttribute("value"), StringComparison.Ordinal);
            Assert.NotNull(cut.Find("[data-testid='editor-profile']"));
            Assert.NotNull(cut.Find("[data-testid='editor-base-wpm']"));
            Assert.NotNull(cut.Find("[data-testid='editor-author']"));
            Assert.NotNull(cut.Find("[data-testid='editor-version']"));
        });
    }

    [Fact]
    public void LearnPage_RendersCenteredRsvpSurfaceAndInitializesTimeline()
    {
        var cut = Render<LearnPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Find("[data-testid='learn-page']"));
            Assert.NotNull(cut.Find("#rsvp-word"));
            Assert.NotNull(cut.Find(".rsvp-orp-line"));
            Assert.NotNull(cut.Find("#rsvp-ctx-l"));
            Assert.NotNull(cut.Find("#rsvp-ctx-r"));
            Assert.Contains("PrompterLiveDesign.setRsvpTimeline", string.Join('\n', _harness.JsRuntime.Invocations));
        });
    }

    [Fact]
    public void TeleprompterPage_RendersSingleBackgroundCameraReaderShell()
    {
        var cut = Render<TeleprompterPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Find("[data-testid='teleprompter-page']"));
            Assert.NotNull(cut.Find("#rd-camera"));
            Assert.NotNull(cut.Find("[data-testid='teleprompter-camera-toggle']"));
            Assert.NotNull(cut.Find(".rd-card-active .rd-cluster-text"));
            Assert.DoesNotContain("rd-camera-overlay-", cut.Markup, StringComparison.Ordinal);
            Assert.Contains("data-total-ms=\"", cut.Markup, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void SettingsPage_RendersOperationalStudioSections()
    {
        var cut = Render<SettingsPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Find("[data-testid='settings-page']"));
            Assert.Contains("Cloud Storage", cut.Markup);
            Assert.Contains("Streaming", cut.Markup);
            Assert.Contains("Audio Sync + Routing", cut.Markup);
            Assert.Contains("Default Camera", cut.Markup);
            Assert.Contains("Broadcast mic", cut.Markup);
        });
    }
}
