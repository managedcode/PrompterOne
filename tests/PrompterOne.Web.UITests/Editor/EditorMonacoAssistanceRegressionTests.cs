using ManagedCode.Tps;
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
    private const string LowEditPointCompletionLabel = "[edit_point:low]";
    private const string MediumEditPointCompletionLabel = "[edit_point:medium]";
    private const string EnergyCompletionLabel = "[energy:8]text[/energy]";
    private const string MelodyCompletionLabel = "[melody:4]text[/melody]";
    private const string LegatoCompletionLabel = "[legato]text[/legato]";
    private const string StaccatoCompletionLabel = "[staccato]text[/staccato]";
    private const string ArchetypeSegmentCompletionLabel = "## [Segment Name|Speaker:Host|Archetype:Coach|neutral|0:00-0:30]";
    private const string ArchetypeLine = "## [Story Arc|Archetype:Storyteller|Warm|0:10-0:45]";
    private const string ArchetypeDocument = """
        # System Design and Software Architecture for Vibe Coders
        ## [Story Arc|Archetype:Storyteller|Warm|0:10-0:45]
        ### [Punchline|Archetype:Entertainer|Excited]
        [breath]
        """;
    private static readonly string[] ExpectedArchetypeSegmentCompletionLabels = TpsSpec.Archetypes
        .Select(archetype => BuildArchetypeSegmentCompletionLabel(ToDisplayLabel(archetype)))
        .ToArray();
    private static readonly string[] ExpectedVendoredWrapperLabels =
        TpsSpec.Emotions.Select(BuildWrapCompletionLabel)
            .Concat(TpsSpec.VolumeLevels.Select(BuildWrapCompletionLabel))
            .Concat(TpsSpec.DeliveryModes.Select(BuildWrapCompletionLabel))
            .Concat(TpsSpec.ArticulationStyles.Select(BuildWrapCompletionLabel))
            .Concat(TpsSpec.RelativeSpeedTags.Select(BuildWrapCompletionLabel))
            .ToArray();

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
            var lowEditPointCompletion = FindCompletion(completions, LowEditPointCompletionLabel);

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
            Assert.Equal("Priority edit point", lowEditPointCompletion.Detail);
            Assert.Contains("low priority", lowEditPointCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_CompletionsExposeNewSdkAuthoringTokensForArchetypesAndVoiceShape()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, "[");
            var completions = await EditorMonacoDriver.GetCompletionsAsync(page, TitleLineNumber, CompletionStartColumn);

            var energyCompletion = FindCompletion(completions, EnergyCompletionLabel);
            var melodyCompletion = FindCompletion(completions, MelodyCompletionLabel);
            var legatoCompletion = FindCompletion(completions, LegatoCompletionLabel);
            var staccatoCompletion = FindCompletion(completions, StaccatoCompletionLabel);
            var archetypeSegmentCompletion = FindCompletion(completions, ArchetypeSegmentCompletionLabel);

            Assert.Equal("Energy contour", energyCompletion.Detail);
            Assert.Contains("1 to 10", energyCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("[energy:${1:8}]${2:text}[/energy]", energyCompletion.InsertText);
            Assert.Equal("Melody contour", melodyCompletion.Detail);
            Assert.Contains("pitch variation", melodyCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("[melody:${1:4}]${2:text}[/melody]", melodyCompletion.InsertText);
            Assert.Contains("legato", legatoCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("staccato", staccatoCompletion.Documentation, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("Segment header (archetype)", archetypeSegmentCompletion.Detail);
            Assert.Contains("Archetype:${3:Coach}", archetypeSegmentCompletion.InsertText, StringComparison.Ordinal);
            foreach (var completionLabel in ExpectedArchetypeSegmentCompletionLabels)
            {
                Assert.NotNull(FindCompletion(completions, completionLabel));
            }
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_CompletionsExposeVendoredEmotionVoiceAndDeliveryWrappers()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, "[");
            var completions = await EditorMonacoDriver.GetCompletionsAsync(page, TitleLineNumber, CompletionStartColumn);

            foreach (var expectedLabel in ExpectedVendoredWrapperLabels)
            {
                Assert.NotNull(FindCompletion(completions, expectedLabel));
            }
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
    public async Task EditorScreen_HoverExplainsArchetypeProfilesFromVendoredSdk()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, ArchetypeDocument);

            var archetypeHover = await EditorMonacoDriver.GetHoverAsync(page, SegmentLineNumber, FindColumn(ArchetypeLine, "Archetype:Storyteller"));

            Assert.NotNull(archetypeHover);
            Assert.Contains(archetypeHover!.Contents, content => content.Contains("Storyteller recommends 125 WPM", StringComparison.Ordinal));
            Assert.Contains(archetypeHover.Contents, content => content.Contains("energy 4-7", StringComparison.Ordinal));
            Assert.Contains(archetypeHover.Contents, content => content.Contains("melody 8-10", StringComparison.Ordinal));
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

    private static string BuildArchetypeSegmentCompletionLabel(string displayName) =>
        $"## [Segment Name|Speaker:Host|Archetype:{displayName}|neutral|0:00-0:30]";

    private static string BuildWrapCompletionLabel(string tagName) =>
        $"[{tagName}]text[/{tagName}]";

    private static string ToDisplayLabel(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : char.ToUpperInvariant(value[0]) + value[1..];
}
