using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

public sealed class EditorFindFlowTests(StandaloneAppFixture fixture) : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    [Fact]
    public Task EditorScreen_FindBarSelectsMatches_AndShowsNoResultState() =>
        RunPageAsync(async page =>
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorQuantum);
            await EditorMonacoDriver.WaitUntilReadyAsync(page);

            await page.GetByTestId(UiTestIds.Editor.FindToggle).ClickAsync();
            await Expect(page.GetByTestId(UiTestIds.Editor.FindBar)).ToBeVisibleAsync();

            await page.GetByTestId(UiTestIds.Editor.FindInput).FillAsync(BrowserTestConstants.Editor.FindQuery);
            await Expect(page.GetByTestId(UiTestIds.Editor.FindResult))
                .ToHaveTextAsync(BrowserTestConstants.Editor.FindSingleMatchSummary);

            var state = await EditorMonacoDriver.GetStateAsync(page);
            var selectedText = state.Text.Substring(
                state.Selection.Start,
                state.Selection.End - state.Selection.Start);
            Assert.Equal(BrowserTestConstants.Editor.FindQuery, selectedText, ignoreCase: true);

            await page.GetByTestId(UiTestIds.Editor.FindInput).FillAsync(BrowserTestConstants.Editor.FindMissingQuery);
            await Expect(page.GetByTestId(UiTestIds.Editor.FindResult))
                .ToHaveTextAsync(BrowserTestConstants.Editor.FindNoMatches);
            await Expect(page.GetByTestId(UiTestIds.Editor.FindNext)).ToBeDisabledAsync();
            await Expect(page.GetByTestId(UiTestIds.Editor.FindPrevious)).ToBeDisabledAsync();
        });
}
