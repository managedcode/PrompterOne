using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class GoLiveShellSessionFlowTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task GoLivePage_StartStream_LeavesPersistentWidgetAndReturnsToActiveSession()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await GoLiveFlowTests.SeedGoLiveSceneForReuseAsync(page);
            await page.GotoAsync(BrowserTestConstants.Routes.GoLiveDemo);
            await page.GetByTestId(UiTestIds.GoLive.SourceCameraSelect(BrowserTestConstants.GoLive.SecondSourceId)).ClickAsync();
            await page.GetByTestId(UiTestIds.GoLive.StartStream).ClickAsync();
            await Expect(page.GetByTestId(UiTestIds.GoLive.ActiveSourceLabel)).ToContainTextAsync(BrowserTestConstants.GoLive.SideCameraLabel);

            await page.GetByTestId(UiTestIds.Header.Back).ClickAsync();
            await Expect(page.GetByTestId(UiTestIds.Library.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.Header.LiveWidget)).ToContainTextAsync(BrowserTestConstants.GoLive.SideCameraLabel);

            await CaptureScreenshotAsync(page, BrowserTestConstants.GoLive.WidgetReturnScreenshotPath);
            await page.GetByTestId(UiTestIds.Header.LiveWidget).ClickAsync();

            await page.WaitForURLAsync(BrowserTestConstants.Routes.Pattern(BrowserTestConstants.Routes.GoLiveDemo));
            await Expect(page.GetByTestId(UiTestIds.GoLive.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.GoLive.ActiveSourceLabel)).ToContainTextAsync(BrowserTestConstants.GoLive.SideCameraLabel);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task GoLivePage_StartRecording_MarksHeaderIndicatorAsRecording()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await GoLiveFlowTests.SeedGoLiveSceneForReuseAsync(page);
            await page.GotoAsync(BrowserTestConstants.Routes.GoLiveDemo);
            await page.GetByTestId(UiTestIds.GoLive.StartRecording).ClickAsync();

            await page.WaitForFunctionAsync(
                BrowserTestConstants.GoLive.RecordingRuntimeActiveScript,
                BrowserTestConstants.GoLive.RuntimeSessionId,
                new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await Expect(page.GetByTestId(UiTestIds.Header.GoLive))
                .ToHaveAttributeAsync("data-live-state", BrowserTestConstants.GoLive.RecordingStateValue);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static async Task CaptureScreenshotAsync(Microsoft.Playwright.IPage page, string relativePath)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await page.ScreenshotAsync(new() { Path = fullPath, FullPage = true });
    }
}
