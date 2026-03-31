using PrompterLive.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterLive.App.UITests;

public sealed class TeleprompterFidelityTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    [Fact]
    public async Task TeleprompterLeadership_RepositionsReadingLineWhenFocalPointChanges()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.TeleprompterLeadership);
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            var focalGuide = page.GetByTestId(UiTestIds.Teleprompter.FocalGuide);
            var firstWord = page.GetByTestId(UiTestIds.Teleprompter.CardWord(0, 0, 0));

            await Expect(firstWord).ToBeVisibleAsync();
            await AssertGuideAlignmentAsync(page, focalGuide, firstWord);

            await page.GetByTestId(UiTestIds.Teleprompter.FocalSlider).EvaluateAsync(
                $$"""
                element => {
                    element.value = '{{BrowserTestConstants.Teleprompter.AdjustedFocalPointPercent}}';
                    element.dispatchEvent(new Event('input', { bubbles: true }));
                }
                """);

            await Expect(focalGuide).ToHaveAttributeAsync("style", BrowserTestConstants.Teleprompter.AdjustedFocalGuideStyle);
            await AssertGuideAlignmentAsync(page, focalGuide, firstWord);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task TeleprompterScreen_UsesSingleFullBleedBackgroundCameraLayer()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.TeleprompterDemo);
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await Expect(page.Locator(BrowserTestConstants.Elements.CameraOverlaySelector)).ToHaveCountAsync(0);
            await EnsureCameraLayerIsActiveAsync(page);

            var isFullBleed = await page.EvaluateAsync<bool>(
                $$"""
                () => {
                    const camera = document.querySelector('[data-testid="teleprompter-camera-layer-primary"]');
                    const shell = document.querySelector('{{BrowserTestConstants.Elements.TeleprompterShellSelector}}');
                    if (!(camera instanceof HTMLElement) || !(shell instanceof HTMLElement)) {
                        return false;
                    }

                    const cameraRect = camera.getBoundingClientRect();
                    const shellRect = shell.getBoundingClientRect();
                    return cameraRect.width >= shellRect.width * 0.95 && cameraRect.height >= shellRect.height * 0.95;
                }
                """);

            Assert.True(isFullBleed);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static async Task EnsureCameraLayerIsActiveAsync(Microsoft.Playwright.IPage page)
    {
        var cameraLayer = page.GetByTestId(UiTestIds.Teleprompter.CameraBackground);
        await TeleprompterCameraDriver.EnsureEnabledAsync(page);
        await Expect(cameraLayer).ToHaveClassAsync(BrowserTestConstants.Regexes.ActiveClass);
    }

    private static async Task AssertGuideAlignmentAsync(
        Microsoft.Playwright.IPage page,
        Microsoft.Playwright.ILocator focalGuide,
        Microsoft.Playwright.ILocator word)
    {
        var attemptCount = BrowserTestConstants.Teleprompter.AlignmentTimeoutMs /
            BrowserTestConstants.Teleprompter.AlignmentPollDelayMs;
        var lastDelta = double.MaxValue;

        for (var attempt = 0; attempt < attemptCount; attempt++)
        {
            lastDelta = await MeasureVerticalCenterDeltaAsync(focalGuide, word);
            if (Math.Abs(lastDelta) <= BrowserTestConstants.Teleprompter.AlignmentTolerancePx)
            {
                return;
            }

            await page.WaitForTimeoutAsync(BrowserTestConstants.Teleprompter.AlignmentPollDelayMs);
        }

        Assert.InRange(
            Math.Abs(lastDelta),
            0,
            BrowserTestConstants.Teleprompter.AlignmentTolerancePx);
    }

    private static async Task<double> MeasureVerticalCenterDeltaAsync(
        Microsoft.Playwright.ILocator focalGuide,
        Microsoft.Playwright.ILocator word)
    {
        var focalGuideBox = await focalGuide.BoundingBoxAsync();
        var wordBox = await word.BoundingBoxAsync();

        Assert.NotNull(focalGuideBox);
        Assert.NotNull(wordBox);

        return (focalGuideBox.Y + (focalGuideBox.Height / 2d)) -
            (wordBox.Y + (wordBox.Height / 2d));
    }
}
