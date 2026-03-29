using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Services.Samples;
using PrompterLive.Shared.Pages;

namespace PrompterLive.Shared.Tests;

public sealed class PageSmokeTests : BunitContext
{
    [Fact]
    public void LibraryPage_RendersSeedScriptsAndStartsNewDraft()
    {
        var harness = TestHarnessFactory.Create(this);

        var cut = Render<LibraryPage>();

        cut.WaitForAssertion(() => Assert.Contains("RSVP Technology Demo", cut.Markup));
        cut.Find("[data-testid='library-new-script']").Click();

        var navigation = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/editor", navigation.Uri, StringComparison.Ordinal);
        Assert.Equal("Fresh Take", harness.Session.State.Title);
    }

    [Fact]
    public void EditorPage_LoadsRequestedScriptAndSupportsToolbarInsertions()
    {
        TestHarnessFactory.Create(this);
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/editor?id={Uri.EscapeDataString(SampleScriptCatalog.DemoSampleId)}");

        var cut = Render<EditorPage>();

        cut.WaitForAssertion(() => Assert.Contains("Opening Block", cut.Markup));
        cut.Find("[data-testid='editor-insert-pause']").Click();

        cut.WaitForAssertion(() => Assert.Contains("[pause:500ms]", cut.Markup));
    }

    [Fact]
    public void LearnPage_UpdatesSpeedSettingsAndPersistsThem()
    {
        var harness = TestHarnessFactory.Create(this);
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/learn?id={Uri.EscapeDataString(SampleScriptCatalog.DemoSampleId)}");

        var cut = Render<LearnPage>();

        cut.WaitForAssertion(() => Assert.Contains("RSVP Technology Demo", cut.Markup));
        cut.Find("[data-testid='learn-wpm']").Input("360");
        cut.WaitForAssertion(() => Assert.Equal(360, harness.Session.State.LearnSettings.WordsPerMinute));

        cut.Find("[data-testid='learn-toggle-speed-mode']").Click();
        cut.WaitForAssertion(() => Assert.True(harness.Session.State.LearnSettings.IgnoreScriptSpeeds));
        Assert.True(harness.JsRuntime.SavedValues.ContainsKey("prompterlive.learn"));
    }

    [Fact]
    public void SettingsPage_RendersStreamingProvidersAndAddsCameraToScene()
    {
        var harness = TestHarnessFactory.Create(this,
        [
            new MediaDeviceInfo("cam-1", "Front camera", MediaDeviceKind.Camera, true),
            new MediaDeviceInfo("mic-1", "Broadcast mic", MediaDeviceKind.Microphone, true)
        ]);

        var cut = Render<SettingsPage>();

        cut.WaitForAssertion(() => Assert.Contains("LiveKit", cut.Markup));
        cut.Find("[data-testid='settings-add-camera-cam-1']").Click();
        cut.WaitForAssertion(() => Assert.Single(harness.SceneService.State.Cameras));

        cut.Find("[data-testid='settings-grant-media']").Click();

        Assert.True(harness.PermissionService.Requested);
        Assert.Contains("Cloud sync UI is rendered", cut.Markup);
    }
}
