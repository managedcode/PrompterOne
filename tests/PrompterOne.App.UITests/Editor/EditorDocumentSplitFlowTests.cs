using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class EditorDocumentSplitFlowTests(StandaloneAppFixture fixture) : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    private const string EpisodeOneCardId = "untitled-script-split-01-episode-1-how-to-think-about-systems";
    private const string EpisodeOneTitle = "Episode 1 - How to Think About Systems";
    private const string EpisodeTwoCardId = "untitled-script-split-02-episode-2-how-systems-talk-to-each-other";
    private const string EpisodeTwoTitle = "Episode 2 - How Systems Talk to Each Other";
    private const string SplitStatusMessage = "2 scripts created from ## headings.";
    private const string SplitSource =
        """
        ## [Episode 1 - How to Think About Systems|140WPM|Professional]
        Before you write code, / you need to think about the system. //

        ## [Episode 2 - How Systems Talk to Each Other|140WPM|Professional]
        APIs, events, and retries matter. //
        """;

    [Fact]
    public Task EditorScreen_SplitBySegmentHeadingCreatesLibraryScriptsWithoutReplacingCurrentDraft() =>
        RunPageAsync(async page =>
        {
            await page.GotoAsync(BrowserTestConstants.Routes.Editor);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page)).ToBeVisibleAsync();
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetTextAsync(page, SplitSource);

            await page.GetByTestId(UiTestIds.Editor.SplitSegment).ClickAsync();

            await Expect(page.GetByTestId(UiTestIds.Editor.SplitStatus)).ToHaveTextAsync(SplitStatusMessage);
            await Expect(EditorMonacoDriver.SourceInput(page)).ToHaveValueAsync(SplitSource);
            Assert.Equal(AppRoutes.Editor, new Uri(page.Url).AbsolutePath);

            await page.GotoAsync(BrowserTestConstants.Routes.Library);
            await Expect(page.GetByTestId(UiTestIds.Library.Page)).ToBeVisibleAsync();
            await page.GetByTestId(UiTestIds.Library.FolderAll).ClickAsync();

            await Expect(page.GetByTestId(UiTestIds.Library.Card(EpisodeOneCardId))).ToContainTextAsync(EpisodeOneTitle);
            await Expect(page.GetByTestId(UiTestIds.Library.Card(EpisodeTwoCardId))).ToContainTextAsync(EpisodeTwoTitle);
        });
}
