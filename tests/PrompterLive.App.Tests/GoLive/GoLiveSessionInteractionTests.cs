using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PrompterLive.Core.Models.Media;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Services;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class GoLiveSessionInteractionTests : BunitContext
{
    private const string SceneSettingsStorageKey = "prompterlive.scene";

    private readonly AppHarness _harness;

    public GoLiveSessionInteractionTests()
    {
        _harness = TestHarnessFactory.Create(this);
    }

    [Fact]
    public void GoLivePage_StartStream_UsesSelectedCameraAsActiveProgramSource()
    {
        SeedSceneState(CreateTwoCameraScene());
        Services.GetRequiredService<NavigationManager>().NavigateTo(AppTestData.Routes.GoLiveDemo);

        var cut = Render<GoLivePage>();

        cut.WaitForAssertion(() => Assert.NotNull(cut.FindByTestId(UiTestIds.GoLive.Page)));
        cut.FindByTestId(UiTestIds.GoLive.SourceCameraSelect(AppTestData.Camera.SecondSourceId)).Click();
        cut.FindByTestId(UiTestIds.GoLive.StartStream).Click();

        var session = Services.GetRequiredService<GoLiveSessionService>().State;
        Assert.True(session.IsStreamActive);
        Assert.Equal(AppTestData.Camera.SecondSourceId, session.ActiveSourceId);
        Assert.Equal(AppTestData.Camera.SideCamera, session.ActiveSourceLabel);
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
                    new MediaSourceTransform(IncludeInOutput: true))
            ],
            "mic-1",
            AppTestData.Scripts.BroadcastMic,
            new AudioBusState([new AudioInputState("mic-1", AppTestData.Scripts.BroadcastMic)]));

    private void SeedSceneState(MediaSceneState sceneState)
    {
        _harness.JsRuntime.SavedJsonValues[SceneSettingsStorageKey] = JsonSerializer.Serialize(sceneState);
    }
}
