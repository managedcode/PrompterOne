using PrompterLive.Core.Models.Workspace;
using PrompterLive.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterLive.App.UITests;

public sealed class GoLiveFlowTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task GoLivePage_ArmsDestinationsAndPersistsValuesInBrowserStorage()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await SeedGoLiveSceneAsync(page);
            await page.GotoAsync(BrowserTestConstants.Routes.GoLiveDemo);
            await Expect(page.GetByTestId(UiTestIds.GoLive.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.GoLive.ProgramCard)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.GoLive.SourcesCard)).ToBeVisibleAsync();

            await page.GetByTestId(UiTestIds.GoLive.LiveKitToggle).ClickAsync();
            await page.GetByTestId(UiTestIds.GoLive.LiveKitServer).FillAsync(BrowserTestConstants.GoLive.LiveKitServer);
            await page.GetByTestId(UiTestIds.GoLive.LiveKitRoom).FillAsync(BrowserTestConstants.GoLive.LiveKitRoom);
            await page.GetByTestId(UiTestIds.GoLive.LiveKitToken).FillAsync(BrowserTestConstants.GoLive.LiveKitToken);
            await page.GetByTestId(UiTestIds.GoLive.YoutubeToggle).ClickAsync();
            await page.GetByTestId(UiTestIds.GoLive.YoutubeUrl).FillAsync(BrowserTestConstants.GoLive.YoutubeUrl);
            await page.GetByTestId(UiTestIds.GoLive.YoutubeKey).FillAsync(BrowserTestConstants.GoLive.YoutubeKey);
            await page.GetByTestId(UiTestIds.GoLive.Bitrate).FillAsync(BrowserTestConstants.Streaming.BitrateKbps);
            await page.GetByTestId(UiTestIds.GoLive.StreamTextOverlay).ClickAsync();

            var liveKitSourceToggle = page.Locator($"[data-testid^='{UiTestIds.GoLive.ProviderSourceToggle(GoLiveTargetCatalog.TargetIds.LiveKit, string.Empty)}']").First;
            var youtubeSourceToggle = page.Locator($"[data-testid^='{UiTestIds.GoLive.ProviderSourceToggle(GoLiveTargetCatalog.TargetIds.Youtube, string.Empty)}']").First;
            await Expect(liveKitSourceToggle).ToBeVisibleAsync();
            await Expect(youtubeSourceToggle).ToBeVisibleAsync();
            await youtubeSourceToggle.ClickAsync();

            await Expect(page.GetByTestId(UiTestIds.GoLive.LiveKitServer)).ToHaveValueAsync(BrowserTestConstants.GoLive.LiveKitServer);
            await Expect(page.GetByTestId(UiTestIds.GoLive.YoutubeUrl)).ToHaveValueAsync(BrowserTestConstants.GoLive.YoutubeUrl);
            await page.WaitForFunctionAsync(
                BrowserTestConstants.GoLive.PersistedTargetsScript,
                new object[]
                {
                    BrowserTestConstants.GoLive.StoredStudioSettingsKey,
                    BrowserTestConstants.GoLive.LiveKitServer,
                    BrowserTestConstants.GoLive.LiveKitRoom,
                    BrowserTestConstants.GoLive.YoutubeUrl,
                    GoLiveTargetCatalog.TargetIds.LiveKit,
                    GoLiveTargetCatalog.TargetIds.Youtube
                },
                new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await page.ReloadAsync(new() { WaitUntil = Microsoft.Playwright.WaitUntilState.NetworkIdle });

            var persistedLiveKitSourceToggle = page.Locator($"[data-testid^='{UiTestIds.GoLive.ProviderSourceToggle(GoLiveTargetCatalog.TargetIds.LiveKit, string.Empty)}']").First;
            var persistedYoutubeSourceToggle = page.Locator($"[data-testid^='{UiTestIds.GoLive.ProviderSourceToggle(GoLiveTargetCatalog.TargetIds.Youtube, string.Empty)}']").First;
            await Expect(persistedLiveKitSourceToggle).ToHaveClassAsync(BrowserTestConstants.Regexes.ToggleOnClass);
            await Expect(persistedYoutubeSourceToggle).Not.ToHaveClassAsync(BrowserTestConstants.Regexes.ToggleOnClass);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task GoLivePage_TogglesSceneCameraMembershipAndLinksBackToRead()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await SeedGoLiveSceneAsync(page);
            await page.GotoAsync(BrowserTestConstants.Routes.GoLiveDemo);
            await Expect(page.GetByTestId(UiTestIds.GoLive.Page)).ToBeVisibleAsync();

            var sourceCamera = page.Locator($"[data-testid^='{UiTestIds.GoLive.SourceCamera(string.Empty)}']").First;
            var sourceButton = sourceCamera.GetByRole(Microsoft.Playwright.AriaRole.Button);
            await Expect(sourceButton).ToContainTextAsync("Remove From Live Feed");
            await sourceButton.ClickAsync();
            await Expect(sourceButton).ToContainTextAsync("Add To Live Feed");
            await sourceButton.ClickAsync();
            await Expect(sourceButton).ToContainTextAsync("Remove From Live Feed");

            await page.GetByTestId(UiTestIds.GoLive.OpenRead).ClickAsync();
            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(BrowserTestConstants.Routes.TeleprompterDemo));
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.Page)).ToBeVisibleAsync();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task GoLivePage_ShowsLiveCameraPreviewForProgramFeed()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await SeedGoLiveSceneAsync(page);
            await page.GotoAsync(BrowserTestConstants.Routes.GoLiveDemo);
            await Expect(page.GetByTestId(UiTestIds.GoLive.Page)).ToBeVisibleAsync();

            var previewCard = page.GetByTestId(UiTestIds.GoLive.PreviewCard);
            var previewVideo = page.GetByTestId(UiTestIds.GoLive.PreviewVideo);

            await Expect(previewCard).ToBeVisibleAsync();
            await Expect(previewVideo).ToBeVisibleAsync();

            var previewHandle = await previewVideo.ElementHandleAsync();
            Assert.NotNull(previewHandle);

            await page.WaitForFunctionAsync(
                BrowserTestConstants.GoLive.PreviewReadyScript,
                previewHandle,
                new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await Expect(page.GetByTestId(UiTestIds.GoLive.PreviewSourceLabel)).ToBeVisibleAsync();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task GoLivePage_ShowsEmptyPreviewStateWhenSceneHasNoCamera()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.Library);
            await Expect(page.GetByTestId(UiTestIds.Library.Page)).ToBeVisibleAsync();
            await page.EvaluateAsync(
                BrowserTestConstants.GoLive.SeedEmptySceneScript,
                BrowserTestConstants.GoLive.SceneStorageKey);

            await page.GotoAsync(BrowserTestConstants.Routes.GoLiveDemo);
            await Expect(page.GetByTestId(UiTestIds.GoLive.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.GoLive.PreviewCard)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.GoLive.PreviewEmpty)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.GoLive.PreviewVideo)).ToHaveCountAsync(0);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static async Task SeedGoLiveSceneAsync(Microsoft.Playwright.IPage page)
    {
        await page.GotoAsync(BrowserTestConstants.Routes.Library);
        await Expect(page.GetByTestId(UiTestIds.Library.Page)).ToBeVisibleAsync();

        var cameraDeviceId = await page.EvaluateAsync<string>(BrowserTestConstants.GoLive.ResolveCameraDeviceScript);
        await page.EvaluateAsync(
            BrowserTestConstants.GoLive.SeedSceneScript,
            new object[]
            {
                BrowserTestConstants.GoLive.SceneStorageKey,
                BrowserTestConstants.GoLive.FirstSourceId,
                BrowserTestConstants.GoLive.SecondSourceId,
                cameraDeviceId
            });
    }

    [Fact]
    public async Task SettingsPage_LinksIntoGoLiveRoutingAndGoLiveLinksBackToSettings()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.Settings);
            await Expect(page.GetByTestId(UiTestIds.Settings.Page)).ToBeVisibleAsync();

            await page.GetByTestId(UiTestIds.Settings.CameraRoutingCta).ClickAsync();
            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(AppRoutes.GoLive));
            await Expect(page.GetByTestId(UiTestIds.GoLive.Page)).ToBeVisibleAsync();

            await page.GetByTestId(UiTestIds.GoLive.OpenSettings).ClickAsync();
            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(BrowserTestConstants.Routes.Settings));
            await Expect(page.GetByTestId(UiTestIds.Settings.Page)).ToBeVisibleAsync();
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
