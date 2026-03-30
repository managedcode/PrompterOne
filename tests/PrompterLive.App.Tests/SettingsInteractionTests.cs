using Bunit;
using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Services;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class SettingsInteractionTests : BunitContext
{
    private const string ReaderSettingsKey = "prompterlive.reader";
    private const string SceneSettingsKey = "prompterlive.scene";

    private readonly AppHarness _harness;

    public SettingsInteractionTests()
    {
        _harness = TestHarnessFactory.Create(this);
    }

    [Fact]
    public void ReaderCameraToggle_UpdatesSessionState_AndPersistsSetting()
    {
        var cut = Render<SettingsPage>();

        cut.WaitForAssertion(() => Assert.Contains(UiTestIds.Settings.ReaderCameraToggle, cut.Markup, StringComparison.Ordinal));

        var initialValue = _harness.Session.State.ReaderSettings.ShowCameraScene;

        cut.FindByTestId(UiTestIds.Settings.ReaderCameraToggle).Click();

        Assert.Equal(!initialValue, _harness.Session.State.ReaderSettings.ShowCameraScene);
        var readerSettings = _harness.JsRuntime.GetSavedValue<ReaderSettings>(ReaderSettingsKey);
        Assert.Equal(!initialValue, readerSettings.ShowCameraScene);
    }

    [Fact]
    public void MicrophoneDelaySlider_UpdatesAudioBusState()
    {
        var cut = Render<SettingsPage>();

        cut.WaitForAssertion(() => Assert.Contains(UiTestIds.Settings.MicDelay("mic-1"), cut.Markup, StringComparison.Ordinal));

        cut.FindByTestId(UiTestIds.Settings.MicDelay("mic-1")).Input(320);

        var audioInput = _harness.SceneService.State.AudioBus.Inputs
            .Single(input => input.DeviceId == "mic-1");

        Assert.Equal(320, audioInput.DelayMs);
        Assert.Equal(AudioRouteTarget.Both, audioInput.RouteTarget);
        var savedScene = _harness.JsRuntime.GetSavedValue<MediaSceneState>(SceneSettingsKey);
        Assert.Contains(savedScene.AudioBus.Inputs, input => input.DeviceId == "mic-1" && input.DelayMs == 320);
    }

    [Fact]
    public void ExactStudioControls_PersistCameraMicAndStreamingPreferences()
    {
        var cut = Render<SettingsPage>();

        cut.WaitForAssertion(() => Assert.Contains(UiTestIds.Settings.DefaultCamera, cut.Markup, StringComparison.Ordinal));

        cut.FindByTestId(UiTestIds.Settings.CameraResolution).Change(CameraResolutionPreset.Hd720.ToString());
        cut.FindByTestId(UiTestIds.Settings.CameraFrameRate).Change(AppTestData.Camera.FrameRateFps24);
        cut.FindByTestId(UiTestIds.Settings.CameraMirrorToggle).Click();
        cut.FindByTestId(UiTestIds.Settings.MicLevel).Input(82);
        cut.FindByTestId(UiTestIds.Settings.NoiseSuppression).Click();

        var settings = _harness.JsRuntime.GetSavedValue<StudioSettings>(StudioSettingsStore.StorageKey);
        Assert.Equal(CameraResolutionPreset.Hd720, settings.Camera.Resolution);
        Assert.Equal(CameraFrameRatePreset.Fps24, settings.Camera.FrameRate);
        Assert.False(settings.Camera.MirrorCamera);
        Assert.Equal(82, settings.Microphone.InputLevelPercent);
        Assert.False(settings.Microphone.NoiseSuppression);
    }
}
