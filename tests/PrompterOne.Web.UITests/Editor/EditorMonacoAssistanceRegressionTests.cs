using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

[Collection(EditorAuthoringCollection.Name)]
public sealed class EditorMonacoAssistanceRegressionTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private const int TitleLineNumber = 1;
    private const int SegmentLineNumber = 2;
    private const int InlineLineNumber = 4;
    private const int CompletionStartColumn = 2;
    private const int CompletionSlashColumn = 3;
    private const int HoverInsideTokenOffset = 2;
    private const string TitleLine = "# System Design and Software Architecture for Vibe Coders";
    private const string SegmentLine = "## [Episode 2 - How Systems Talk to Each Other|140WPM|Professional|Speaker:Alex|0:00-0:30]";
    private const string InlineGuideLine = "[edit_point:high] [pause:1500ms] [pronunciation:KAM-uhl]teleprompter[/pronunciation]";
    private const string SimpleSegmentLine = "## Segment Title";
    private const string SimpleBlockLine = "### Block Title";
    private const string TokenizationLine = @"Escaped slash \/ pipe \| star \* slash \\ should stay literal [180WPM] [normal]steady[/normal] *calm* **loud**";
    private const string MetadataDocument = """
        # System Design and Software Architecture for Vibe Coders
        ## [Episode 2 - How Systems Talk to Each Other|140WPM|Professional|Speaker:Alex|0:00-0:30]
        ### [Connection Patterns|145WPM|Warm]
        [edit_point:high] [pause:1500ms] [pronunciation:KAM-uhl]teleprompter[/pronunciation]
        """;
    private const string SimpleHeaderDocument = """
        ## Segment Title
        ### Block Title
        Escaped slash \/ pipe \| star \* slash \\ should stay literal [180WPM] [normal]steady[/normal] *calm* **loud**
        """;
    private const string PronunciationCompletionLabel = "[pronunciation:guide]text[/pronunciation]";
    private const string SegmentCompletionLabel = "## [Segment Name|Speaker:Host|140WPM|neutral|0:00-0:30]";
    private const string TimedPauseCompletionLabel = "[pause:2s]";
    private const string MarkdownBoldCompletionLabel = "**text**";
    private const string MarkdownItalicCompletionLabel = "*text*";
    private const string MillisecondPauseCompletionLabel = "[pause:1000ms]";
    private const string MediumEditPointCompletionLabel = "[edit_point:medium]";

    [Fact]
    public async Task EditorScreen_CompletionsExposeDetailedPayloadsForStructuredTpsSuggestions()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, "[");
            var completions = await EditorMonacoDriver.GetCompletionsAsync(page, TitleLineNumber, CompletionStartColumn);

            var pronunciationCompletion = FindCompletion(completions, PronunciationCompletionLabel);
            var segmentCompletion = FindCompletion(completions, SegmentCompletionLabel);
            var markdownBoldCompletion = FindCompletion(completions, MarkdownBoldCompletionLabel);
            var markdownItalicCompletion = FindCompletion(completions, MarkdownItalicCompletionLabel);
            var millisecondPauseCompletion = FindCompletion(completions, MillisecondPauseCompletionLabel);
            var mediumEditPointCompletion = FindCompletion(completions, MediumEditPointCompletionLabel);

            Assert.Equal("Simple pronunciation guide", pronunciationCompletion.Detail);
            Assert.Contains("readable pronunciation guide", pronunciationCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("[pronunciation:${1:KAM-uhl}]${2:camel}[/pronunciation]", pronunciationCompletion.InsertText);
            Assert.Equal("Segment header", segmentCompletion.Detail);
            Assert.Contains("structured TPS segment header", segmentCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("## [${1:Segment Name}|Speaker:${2:Host}|${3:140}WPM|${4:neutral}|${5:0:00-0:30}]", segmentCompletion.InsertText);
            Assert.Equal("Markdown bold", markdownBoldCompletion.Detail);
            Assert.Equal("**${1:text}**", markdownBoldCompletion.InsertText);
            Assert.Equal("Markdown italic", markdownItalicCompletion.Detail);
            Assert.Equal("*${1:text}*", markdownItalicCompletion.InsertText);
            Assert.Equal("Timed pause (ms)", millisecondPauseCompletion.Detail);
            Assert.Contains("milliseconds", millisecondPauseCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("Priority edit point", mediumEditPointCompletion.Detail);
            Assert.Contains("medium priority", mediumEditPointCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_CompletionsStayContextAwareForPauseInsertion()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, " /");
            var completions = await EditorMonacoDriver.GetCompletionsAsync(page, TitleLineNumber, CompletionSlashColumn);

            var shortPause = FindCompletion(completions, "/");
            var mediumPause = FindCompletion(completions, "//");
            var timedPause = FindCompletion(completions, TimedPauseCompletionLabel);

            Assert.Equal("Short pause", shortPause.Detail);
            Assert.Equal(" / ", shortPause.InsertText);
            Assert.Equal("Medium pause", mediumPause.Detail);
            Assert.Equal(" //", mediumPause.InsertText);
            Assert.Equal("Timed pause", timedPause.Detail);
            Assert.Contains("seconds or milliseconds", timedPause.Documentation, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_HoverExplainsTitleAndStructuredHeaderMetadata()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, MetadataDocument);

            var titleHover = await EditorMonacoDriver.GetHoverAsync(page, TitleLineNumber, FindColumn(TitleLine, "#"));
            var wpmHover = await EditorMonacoDriver.GetHoverAsync(page, SegmentLineNumber, FindColumn(SegmentLine, "140WPM"));
            var emotionHover = await EditorMonacoDriver.GetHoverAsync(page, SegmentLineNumber, FindColumn(SegmentLine, "Professional"));
            var timingHover = await EditorMonacoDriver.GetHoverAsync(page, SegmentLineNumber, FindColumn(SegmentLine, "0:00-0:30"));

            Assert.NotNull(titleHover);
            Assert.NotNull(wpmHover);
            Assert.NotNull(emotionHover);
            Assert.NotNull(timingHover);
            Assert.Contains(titleHover!.Contents, content => content.Contains("Document title", StringComparison.Ordinal));
            Assert.Contains(wpmHover!.Contents, content => content.Contains("WPM override", StringComparison.Ordinal));
            Assert.Contains(emotionHover!.Contents, content => content.Contains("Emotion override", StringComparison.Ordinal));
            Assert.Contains(timingHover!.Contents, content => content.Contains("Optional timing window", StringComparison.Ordinal));
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_HoverExplainsEditPointPriorityAndTimedPauseTags()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, MetadataDocument);

            var editPointHover = await EditorMonacoDriver.GetHoverAsync(page, InlineLineNumber, FindColumn(InlineGuideLine, "[edit_point:high]"));
            var timedPauseHover = await EditorMonacoDriver.GetHoverAsync(page, InlineLineNumber, FindColumn(InlineGuideLine, "[pause:1500ms]"));

            Assert.NotNull(editPointHover);
            Assert.NotNull(timedPauseHover);
            Assert.Contains(editPointHover!.Contents, content => content.Contains("Edit point", StringComparison.Ordinal) &&
                content.Contains("high", StringComparison.Ordinal));
            Assert.Contains(timedPauseHover!.Contents, content => content.Contains("Timed pause", StringComparison.Ordinal) &&
                content.Contains("1500ms", StringComparison.Ordinal));
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_TokenizesSimpleHeadersEscapesAndInlineWpmBadges()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, SimpleHeaderDocument);

            var simpleSegmentTokens = await EditorMonacoDriver.TokenizeLineAsync(page, TitleLineNumber);
            var simpleBlockTokens = await EditorMonacoDriver.TokenizeLineAsync(page, SegmentLineNumber);
            var edgeCaseTokens = await EditorMonacoDriver.TokenizeLineAsync(page, 3);

            Assert.Equal(SimpleSegmentLine, simpleSegmentTokens.LineText);
            Assert.Equal(SimpleBlockLine, simpleBlockTokens.LineText);
            Assert.Equal(TokenizationLine, edgeCaseTokens.LineText);
            Assert.Contains(simpleSegmentTokens.Tokens, token => token.Type.Contains("header.segment.hash", StringComparison.Ordinal));
            Assert.Contains(simpleBlockTokens.Tokens, token => token.Type.Contains("header.block.hash", StringComparison.Ordinal));
            Assert.Contains(edgeCaseTokens.Tokens, token => token.Type.Contains("escape.sequence", StringComparison.Ordinal));
            Assert.Contains(edgeCaseTokens.Tokens, token => token.Type.Contains("wpm.badge", StringComparison.Ordinal));
            Assert.Contains(edgeCaseTokens.Tokens, token => token.Type.Contains("cue.open", StringComparison.Ordinal));
            Assert.Contains(edgeCaseTokens.Tokens, token => token.Type.Contains("cue.close", StringComparison.Ordinal));
            Assert.Contains(edgeCaseTokens.Tokens, token => token.Type.Contains("markdown.italic", StringComparison.Ordinal));
            Assert.Contains(edgeCaseTokens.Tokens, token => token.Type.Contains("markdown.bold", StringComparison.Ordinal));
            Assert.DoesNotContain(edgeCaseTokens.Tokens, token => token.Type.Contains("pause.short", StringComparison.Ordinal));
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_HoverExplainsMarkdownItalicSyntax()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, SimpleHeaderDocument);

            var italicHover = await EditorMonacoDriver.GetHoverAsync(page, 3, FindColumn(TokenizationLine, "*calm*"));

            Assert.NotNull(italicHover);
            Assert.Contains(italicHover!.Contents, content => content.Contains("Markdown italic", StringComparison.Ordinal));
            Assert.Contains(italicHover.Contents, content => content.Contains("*text*", StringComparison.Ordinal));
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private async Task<Microsoft.Playwright.IPage> OpenEditorAsync()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync(BrowserTestConstants.Routes.EditorDemo);
        await Expect(page.GetByTestId(UiTestIds.Editor.Page))
            .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
        await EditorMonacoDriver.WaitUntilReadyAsync(page);
        return page;
    }

    private static EditorMonacoCompletionItem FindCompletion(EditorMonacoCompletionList completions, string label)
    {
        var completion = completions.Suggestions.SingleOrDefault(suggestion => string.Equals(suggestion.Label, label, StringComparison.Ordinal));
        Assert.NotNull(completion);
        return completion!;
    }

    private static int FindColumn(string line, string fragment)
    {
        var index = line.IndexOf(fragment, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Unable to locate \"{fragment}\" inside the Monaco regression probe line.");
        return index + HoverInsideTokenOffset;
    }
}
