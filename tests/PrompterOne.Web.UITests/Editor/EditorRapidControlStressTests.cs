using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

public sealed class EditorRapidControlStressTests(StandaloneAppFixture fixture)
    : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    private const string RapidControlsScenario = "editor-rapid-controls";
    private const string RapidControlsStep = "01-rapid-toolbar-history";

    [Fact]
    public Task Editor_RapidToolbarAndHistoryClicks_DoNotTriggerFatalDiagnosticsOrUnhandledErrors() =>
        RunPageAsync(async page =>
        {
            UiScenarioArtifacts.ResetScenario(RapidControlsScenario);
            var errors = BrowserErrorCollector.Attach(page);

            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetCaretAtEndAsync(page);

            await page.GetByTestId(UiTestIds.Editor.PauseTrigger).ClickAsync();
            await Expect(page.GetByTestId(UiTestIds.Editor.MenuPause)).ToBeVisibleAsync();
            await page.GetByTestId(UiTestIds.Editor.PauseTwoSeconds).ClickAsync();

            await RapidControlDriver.BurstClickAsync(page, UiTestIds.Editor.Undo, BrowserTestConstants.RapidInput.EditorHistoryBurstCount);
            await RapidControlDriver.BurstClickAsync(page, UiTestIds.Editor.Redo, BrowserTestConstants.RapidInput.EditorHistoryBurstCount);

            await page.GetByTestId(UiTestIds.Editor.PauseTrigger).ClickAsync();
            await page.GetByTestId(UiTestIds.Editor.PauseTwoSeconds).ClickAsync();
            await page.WaitForTimeoutAsync(BrowserTestConstants.RapidInput.PostBurstSettleDelayMs);

            await Expect(page.GetByTestId(UiTestIds.Editor.Page)).ToBeVisibleAsync();
            await Expect(EditorMonacoDriver.SourceInput(page)).ToHaveValueAsync(BrowserTestConstants.Regexes.EndsWithPause);
            Assert.Equal(0, await page.GetByTestId(UiTestIds.Diagnostics.Fatal).CountAsync());
            errors.AssertNoCriticalUiErrors();

            await UiScenarioArtifacts.CapturePageAsync(page, RapidControlsScenario, RapidControlsStep);
        });
}
