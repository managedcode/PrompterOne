using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class LearnRapidControlStressTests(StandaloneAppFixture fixture)
    : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    private const string RapidControlsScenario = "learn-rapid-controls";
    private const string RapidControlsStep = "01-rapid-controls";

    [Fact]
    public Task Learn_RapidControlClicks_DoNotTriggerFatalDiagnosticsOrUnhandledErrors() =>
        RunPageAsync(async page =>
        {
            UiScenarioArtifacts.ResetScenario(RapidControlsScenario);
            var errors = BrowserErrorCollector.Attach(page);

            await page.GotoAsync(BrowserTestConstants.Routes.LearnDemo);
            await Expect(page.GetByTestId(UiTestIds.Learn.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await RapidControlDriver.BurstClickAsync(page, UiTestIds.Learn.StepForward, BrowserTestConstants.RapidInput.LearnStepBurstCount);
            await RapidControlDriver.BurstClickAsync(page, UiTestIds.Learn.StepBackward, BrowserTestConstants.RapidInput.LearnStepBurstCount);
            await RapidControlDriver.BurstClickAsync(page, UiTestIds.Learn.PlayToggle, BrowserTestConstants.RapidInput.LearnPlayToggleBurstCount);
            await RapidControlDriver.BurstClickAsync(page, UiTestIds.Learn.LoopToggle, BrowserTestConstants.RapidInput.LearnLoopToggleBurstCount);
            await page.WaitForTimeoutAsync(BrowserTestConstants.RapidInput.PostBurstSettleDelayMs);

            await Expect(page.GetByTestId(UiTestIds.Learn.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(UiTestIds.Learn.ProgressLabel)).ToBeVisibleAsync();
            Assert.Equal(0, await page.GetByTestId(UiTestIds.Diagnostics.Fatal).CountAsync());
            errors.AssertNoCriticalUiErrors();

            await UiScenarioArtifacts.CapturePageAsync(page, RapidControlsScenario, RapidControlsStep);
        });
}
