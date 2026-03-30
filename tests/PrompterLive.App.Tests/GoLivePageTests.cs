using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Services;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class GoLivePageTests : BunitContext
{
    private const string SceneSettingsStorageKey = "prompterlive.scene";

    private readonly AppHarness _harness;

    public GoLivePageTests()
    {
        _harness = TestHarnessFactory.Create(this);
    }

    [Fact]
    public void GoLivePage_PersistsMultipleDestinationsAndProgramSettings()
    {
        SeedSceneState(CreateTwoCameraScene());

        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppTestData.Routes.GoLiveDemo);

        var cut = Render<GoLivePage>();

        cut.WaitForAssertion(() => Assert.Contains(UiTestIds.GoLive.Page, cut.Markup, StringComparison.Ordinal));

        cut.FindByTestId(UiTestIds.GoLive.LiveKitToggle).Click();
        cut.FindByTestId(UiTestIds.GoLive.LiveKitServer).Input(AppTestData.GoLive.LiveKitServer);
        cut.FindByTestId(UiTestIds.GoLive.LiveKitRoom).Input(AppTestData.GoLive.LiveKitRoom);
        cut.FindByTestId(UiTestIds.GoLive.LiveKitToken).Input(AppTestData.GoLive.LiveKitToken);
        cut.FindByTestId(UiTestIds.GoLive.YoutubeToggle).Click();
        cut.FindByTestId(UiTestIds.GoLive.YoutubeUrl).Input(AppTestData.GoLive.YoutubeUrl);
        cut.FindByTestId(UiTestIds.GoLive.YoutubeKey).Input(AppTestData.GoLive.YoutubeKey);
        cut.FindByTestId(UiTestIds.GoLive.Bitrate).Input(AppTestData.Streaming.BitrateKbps);

        var settings = _harness.JsRuntime.GetSavedValue<StudioSettings>(StudioSettingsStore.StorageKey);

        Assert.True(settings.Streaming.LiveKitEnabled);
        Assert.Equal(AppTestData.GoLive.LiveKitServer, settings.Streaming.LiveKitServerUrl);
        Assert.Equal(AppTestData.GoLive.LiveKitRoom, settings.Streaming.LiveKitRoomName);
        Assert.Equal(AppTestData.GoLive.LiveKitToken, settings.Streaming.LiveKitToken);
        Assert.True(settings.Streaming.YoutubeEnabled);
        Assert.Equal(AppTestData.GoLive.YoutubeUrl, settings.Streaming.YoutubeRtmpUrl);
        Assert.Equal(AppTestData.GoLive.YoutubeKey, settings.Streaming.YoutubeStreamKey);
        Assert.Equal(AppTestData.Streaming.BitrateKbps, settings.Streaming.BitrateKbps);
    }

    [Fact]
    public void GoLivePage_TogglesProgramSourcesAndPersistsScene()
    {
        SeedSceneState(CreateTwoCameraScene());

        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppTestData.Routes.GoLiveDemo);

        var cut = Render<GoLivePage>();

        cut.WaitForAssertion(() => Assert.Contains(UiTestIds.GoLive.SourcesCard, cut.Markup, StringComparison.Ordinal));
        cut.WaitForAssertion(() =>
            Assert.NotEmpty(cut.FindAll($"[data-testid^='{UiTestIds.GoLive.SourceCamera(string.Empty)}']")));

        cut.FindByTestId(UiTestIds.GoLive.SourceCameraAction(AppTestData.Camera.FirstDeviceId)).Click();

        var sceneState = _harness.JsRuntime.GetSavedValue<MediaSceneState>("prompterlive.scene");
        Assert.False(
            sceneState.Cameras.Single(camera => camera.SourceId == AppTestData.Camera.FirstSourceId).Transform.IncludeInOutput);
    }

    [Fact]
    public void GoLivePage_LoadsPersistedDestinationsFromBrowserStorage()
    {
        SeedSceneState(CreateTwoCameraScene());
        _harness.JsRuntime.SavedValues[StudioSettingsStore.StorageKey] = StudioSettings.Default with
        {
            Streaming = StudioSettings.Default.Streaming with
            {
                LiveKitEnabled = true,
                LiveKitServerUrl = AppTestData.GoLive.LiveKitServer,
                LiveKitRoomName = AppTestData.GoLive.LiveKitRoom,
                LiveKitToken = AppTestData.GoLive.LiveKitToken,
                DestinationSourceSelections =
                [
                    new GoLiveDestinationSourceSelection(
                        GoLiveTargetCatalog.TargetIds.LiveKit,
                        [AppTestData.Camera.SecondSourceId])
                ],
                YoutubeEnabled = true,
                YoutubeRtmpUrl = AppTestData.GoLive.YoutubeUrl,
                YoutubeStreamKey = AppTestData.GoLive.YoutubeKey,
                BitrateKbps = AppTestData.Streaming.BitrateKbps
            }
        };

        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppTestData.Routes.GoLiveDemo);

        var cut = Render<GoLivePage>();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                AppTestData.GoLive.LiveKitServer,
                cut.FindByTestId(UiTestIds.GoLive.LiveKitServer).GetAttribute("value"));
            Assert.Equal(
                AppTestData.GoLive.LiveKitRoom,
                cut.FindByTestId(UiTestIds.GoLive.LiveKitRoom).GetAttribute("value"));
            Assert.Equal(
                AppTestData.GoLive.YoutubeUrl,
                cut.FindByTestId(UiTestIds.GoLive.YoutubeUrl).GetAttribute("value"));
            Assert.Contains(
                "on",
                cut.FindByTestId(UiTestIds.GoLive.LiveKitToggle).ClassName,
                StringComparison.Ordinal);
            Assert.Contains(
                "on",
                cut.FindByTestId(UiTestIds.GoLive.YoutubeToggle).ClassName,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "on",
                cut.FindByTestId(UiTestIds.GoLive.ProviderSourceToggle(
                    GoLiveTargetCatalog.TargetIds.LiveKit,
                    AppTestData.Camera.FirstSourceId)).ClassName!.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                StringComparer.Ordinal);
            Assert.Contains(
                "on",
                cut.FindByTestId(UiTestIds.GoLive.ProviderSourceToggle(
                    GoLiveTargetCatalog.TargetIds.LiveKit,
                    AppTestData.Camera.SecondSourceId)).ClassName!.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                StringComparer.Ordinal);
        });
    }

    [Fact]
    public void GoLivePage_PersistsPerDestinationSourceRouting()
    {
        SeedSceneState(CreateTwoCameraScene());

        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppTestData.Routes.GoLiveDemo);

        var cut = Render<GoLivePage>();

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.FindByTestId(UiTestIds.GoLive.ProviderSourcePicker(GoLiveTargetCatalog.TargetIds.LiveKit))));

        cut.FindByTestId(UiTestIds.GoLive.ProviderSourceToggle(
            GoLiveTargetCatalog.TargetIds.LiveKit,
            AppTestData.Camera.SecondSourceId)).Click();
        cut.FindByTestId(UiTestIds.GoLive.ProviderSourceToggle(
            GoLiveTargetCatalog.TargetIds.Youtube,
            AppTestData.Camera.SecondSourceId)).Click();
        cut.FindByTestId(UiTestIds.GoLive.ProviderSourceToggle(
            GoLiveTargetCatalog.TargetIds.Youtube,
            AppTestData.Camera.FirstSourceId)).Click();

        var settings = _harness.JsRuntime.GetSavedValue<StudioSettings>(StudioSettingsStore.StorageKey);
        var liveKitSelection = settings.Streaming.DestinationSourceSelections!
            .Single(selection => selection.TargetId == GoLiveTargetCatalog.TargetIds.LiveKit);
        var youtubeSelection = settings.Streaming.DestinationSourceSelections!
            .Single(selection => selection.TargetId == GoLiveTargetCatalog.TargetIds.Youtube);

        Assert.Equal([AppTestData.Camera.FirstSourceId, AppTestData.Camera.SecondSourceId], liveKitSelection.SourceIds);
        Assert.Equal([AppTestData.Camera.SecondSourceId], youtubeSelection.SourceIds);
    }

    private static MediaSceneState CreateTwoCameraScene() =>
        new(
            [
                new SceneCameraSource(
                    AppTestData.Camera.FirstSourceId,
                    AppTestData.Camera.FirstDeviceId,
                    AppTestData.Camera.FrontCamera,
                    new MediaSourceTransform(IncludeInOutput: true)),
                new SceneCameraSource(
                    AppTestData.Camera.SecondSourceId,
                    AppTestData.Camera.SecondDeviceId,
                    AppTestData.Camera.SideCamera,
                    new MediaSourceTransform(IncludeInOutput: false))
            ],
            "mic-1",
            AppTestData.Scripts.BroadcastMic,
            new AudioBusState([new AudioInputState("mic-1", AppTestData.Scripts.BroadcastMic)]));

    private void SeedSceneState(MediaSceneState sceneState)
    {
        _harness.JsRuntime.SavedJsonValues[SceneSettingsStorageKey] = JsonSerializer.Serialize(sceneState);
    }
}
