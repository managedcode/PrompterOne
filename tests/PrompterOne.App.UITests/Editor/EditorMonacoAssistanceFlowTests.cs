using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class EditorMonacoAssistanceFlowTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private const int TitleLineNumber = 1;
    private const int SegmentLineNumber = 2;
    private const int BlockLineNumber = 3;
    private const int InlineLineNumber = 4;
    private const int CompletionInvokeLineNumber = 1;
    private const int CompletionInvokeColumn = 2;
    private const int HoverInsideTokenOffset = 2;
    private const string TitleLine = "# System Design and Software Architecture for Vibe Coders";
    private const string SegmentLine = "## [Episode 2 - How Systems Talk to Each Other|140WPM|Professional|Speaker:Alex]";
    private const string BlockLine = "### Connection Patterns";
    private const string InlineLine = "[breath] [edit_point:high] [phonetic:ˈkæməl]camel[/phonetic] [pronunciation:KAM-uhl]teleprompter[/pronunciation] [stress:de-VE-lop-ment]development[/stress] / // [pause:2s]";
    private const string AssistanceDocument = """
        # System Design and Software Architecture for Vibe Coders
        ## [Episode 2 - How Systems Talk to Each Other|140WPM|Professional|Speaker:Alex]
        ### Connection Patterns
        [breath] [edit_point:high] [phonetic:ˈkæməl]camel[/phonetic] [pronunciation:KAM-uhl]teleprompter[/pronunciation] [stress:de-VE-lop-ment]development[/stress] / // [pause:2s]
        """;
    private static readonly string[] ExpectedCompletionLabels =
    [
        "# Title",
        "## [Segment Name|Speaker:Host|140WPM|neutral|0:00-0:30]",
        "### [Block Name|Speaker:Host|140WPM|focused]",
        "[breath]",
        "[edit_point:high]",
        "[phonetic:guide]text[/phonetic]",
        "[pronunciation:guide]text[/pronunciation]",
        "[stress:guide]text[/stress]",
        "[normal]text[/normal]",
        "/",
        "//"
    ];

    [Fact]
    public async Task EditorScreen_TokenizesSupportedTpsSyntaxWithMonacoLanguage()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetTextAsync(page, AssistanceDocument);

            var titleTokens = await EditorMonacoDriver.TokenizeLineAsync(page, TitleLineNumber);
            var segmentTokens = await EditorMonacoDriver.TokenizeLineAsync(page, SegmentLineNumber);
            var blockTokens = await EditorMonacoDriver.TokenizeLineAsync(page, BlockLineNumber);
            var inlineTokens = await EditorMonacoDriver.TokenizeLineAsync(page, InlineLineNumber);

            Assert.Equal(TitleLine, titleTokens.LineText);
            Assert.Equal(SegmentLine, segmentTokens.LineText);
            Assert.Equal(BlockLine, blockTokens.LineText);
            Assert.Equal(InlineLine, inlineTokens.LineText);
            Assert.Contains(titleTokens.Tokens, token => token.Type.Contains("header.title.hash", StringComparison.Ordinal));
            Assert.Contains(titleTokens.Tokens, token => token.Type.Contains("header.title.body", StringComparison.Ordinal));
            Assert.Contains(segmentTokens.Tokens, token => token.Type.Contains("header.segment.hash", StringComparison.Ordinal));
            Assert.Contains(segmentTokens.Tokens, token => token.Type.Contains("header.segment.body", StringComparison.Ordinal));
            Assert.Contains(blockTokens.Tokens, token => token.Type.Contains("header.block.hash", StringComparison.Ordinal));
            Assert.Contains(blockTokens.Tokens, token => token.Type.Contains("header.block.body", StringComparison.Ordinal));
            Assert.Contains(inlineTokens.Tokens, token => token.Type.Contains("cue.breath", StringComparison.Ordinal));
            Assert.Contains(inlineTokens.Tokens, token => token.Type.Contains("cue.editpoint", StringComparison.Ordinal));
            Assert.Contains(inlineTokens.Tokens, token => token.Type.Contains("cue.pronunciation", StringComparison.Ordinal));
            Assert.Contains(inlineTokens.Tokens, token => token.Type.Contains("pause.short", StringComparison.Ordinal));
            Assert.Contains(inlineTokens.Tokens, token => token.Type.Contains("pause.long", StringComparison.Ordinal));
            Assert.Contains(inlineTokens.Tokens, token => token.Type.Contains("pause.timed", StringComparison.Ordinal));
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_ProvidesMonacoTpsCompletions()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetTextAsync(page, "[");

            var completions = await EditorMonacoDriver.GetCompletionsAsync(page, CompletionInvokeLineNumber, CompletionInvokeColumn);

            Assert.NotEmpty(completions.Suggestions);
            foreach (var expectedLabel in ExpectedCompletionLabels)
            {
                Assert.Contains(completions.Suggestions, suggestion => string.Equals(suggestion.Label, expectedLabel, StringComparison.Ordinal));
            }
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_ProvidesMonacoHoverHelpForTpsAuthoring()
    {
        var page = await fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
            await Expect(page.GetByTestId(UiTestIds.Editor.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
            await EditorMonacoDriver.WaitUntilReadyAsync(page);
            await EditorMonacoDriver.SetTextAsync(page, AssistanceDocument);

            var breathHover = await EditorMonacoDriver.GetHoverAsync(page, InlineLineNumber, FindColumn(InlineLine, "[breath]"));
            var speakerHover = await EditorMonacoDriver.GetHoverAsync(page, SegmentLineNumber, FindColumn(SegmentLine, "Speaker:Alex"));
            var pauseHover = await EditorMonacoDriver.GetHoverAsync(page, InlineLineNumber, FindColumn(InlineLine, "//"));
            var guideHover = await EditorMonacoDriver.GetHoverAsync(page, InlineLineNumber, FindColumn(InlineLine, "development"));

            Assert.NotNull(breathHover);
            Assert.NotNull(speakerHover);
            Assert.NotNull(pauseHover);
            Assert.NotNull(guideHover);
            Assert.Contains(breathHover!.Contents, content => content.Contains("Breath mark", StringComparison.Ordinal) &&
                content.Contains("natural breath point", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(speakerHover!.Contents, content => content.Contains("Talent assignment", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(pauseHover!.Contents, content => content.Contains("Medium pause", StringComparison.Ordinal) &&
                content.Contains("600ms", StringComparison.Ordinal));
            Assert.Contains(guideHover!.Contents, content => content.Contains("Syllable guide", StringComparison.Ordinal) &&
                content.Contains("de-VE-lop-ment", StringComparison.Ordinal));
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static int FindColumn(string line, string fragment)
    {
        var index = line.IndexOf(fragment, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Unable to locate \"{fragment}\" inside the Monaco assistance probe line.");
        return index + HoverInsideTokenOffset;
    }
}
