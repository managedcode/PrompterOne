using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

public sealed class TeleprompterKeyboardShortcutFlowTests(StandaloneAppFixture fixture)
    : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    [Fact]
    public Task TeleprompterPage_KeyboardShortcuts_ToggleMirrorAndJustifyAlignment() =>
        RunPageAsync(async page =>
        {
            UiScenarioArtifacts.ResetScenario(BrowserTestConstants.Teleprompter.ShortcutScenarioName);

            await page.GotoAsync(BrowserTestConstants.Routes.TeleprompterDemo);
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            await page.GetByTestId(UiTestIds.Teleprompter.Page).FocusAsync();
            await page.Keyboard.PressAsync(BrowserTestConstants.Keyboard.H);
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.MirrorHorizontalToggle))
                .ToHaveClassAsync(BrowserTestConstants.Regexes.ActiveClass);

            await page.Keyboard.PressAsync(BrowserTestConstants.Keyboard.Digit4);
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.AlignmentJustify))
                .ToHaveClassAsync(BrowserTestConstants.Regexes.ActiveClass);
            await Expect(page.GetByTestId(UiTestIds.Teleprompter.ClusterWrap))
                .ToHaveAttributeAsync(
                    BrowserTestConstants.TeleprompterFlow.ReaderTextAlignmentAttribute,
                    BrowserTestConstants.TeleprompterFlow.AlignmentJustifyValue);

            await UiScenarioArtifacts.CapturePageAsync(
                page,
                BrowserTestConstants.Teleprompter.ShortcutScenarioName,
                BrowserTestConstants.Teleprompter.ShortcutStep);
        });
}
