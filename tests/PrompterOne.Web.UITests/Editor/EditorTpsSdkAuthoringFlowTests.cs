using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

[Collection(EditorAuthoringCollection.Name)]
public sealed class EditorTpsSdkAuthoringFlowTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private const string MinimalDocument = "# TPS Authoring";
    private const string SelectionDocument = """
        ## [Intro|140WPM|warm]
        ### [Opening Block|140WPM]
        Alpha
        """;
    private const string SelectionToken = "Alpha";

    [Fact]
    public async Task EditorScreen_InsertMenuAddsArchetypeAwareSegmentHeader()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, MinimalDocument);
            await EditorMonacoDriver.SetCaretAtEndAsync(page);
            await page.GetByTestId(UiTestIds.Editor.InsertTrigger).ClickAsync();
            await page.GetByTestId(UiTestIds.Editor.InsertSegmentArchetypeMenu).ClickAsync();

            var source = await EditorMonacoDriver.SourceInput(page).InputValueAsync();

            Assert.Contains("Archetype:Coach", source, StringComparison.Ordinal);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_InsertMenuAddsArchetypeAwareBlockHeader()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, MinimalDocument);
            await EditorMonacoDriver.SetCaretAtEndAsync(page);
            await page.GetByTestId(UiTestIds.Editor.InsertTrigger).ClickAsync();
            await page.GetByTestId(UiTestIds.Editor.InsertBlockArchetypeMenu).ClickAsync();

            var source = await EditorMonacoDriver.SourceInput(page).InputValueAsync();

            Assert.Contains("Archetype:Educator", source, StringComparison.Ordinal);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_FloatingVoiceMenuAppliesEnergyTag()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, SelectionDocument);
            await EditorMonacoDriver.SetSelectionByTextAsync(page, SelectionToken);
            await Expect(page.GetByTestId(UiTestIds.Editor.FloatingBar))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.FastVisibleTimeoutMs });
            await page.GetByTestId(UiTestIds.Editor.FloatingVoice).ClickAsync();
            await page.GetByTestId(UiTestIds.Editor.FloatingVoiceEnergy).ClickAsync();

            var source = await EditorMonacoDriver.SourceInput(page).InputValueAsync();

            Assert.Contains("[energy:8]Alpha[/energy]", source, StringComparison.Ordinal);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_FloatingVoiceMenuAppliesLegatoTag()
    {
        var page = await OpenEditorAsync();

        try
        {
            await EditorMonacoDriver.SetTextAsync(page, SelectionDocument);
            await EditorMonacoDriver.SetSelectionByTextAsync(page, SelectionToken);
            await Expect(page.GetByTestId(UiTestIds.Editor.FloatingBar))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.FastVisibleTimeoutMs });
            await page.GetByTestId(UiTestIds.Editor.FloatingVoice).ClickAsync();
            await page.GetByTestId(UiTestIds.Editor.FloatingVoiceLegato).ClickAsync();

            var source = await EditorMonacoDriver.SourceInput(page).InputValueAsync();

            Assert.Contains("[legato]Alpha[/legato]", source, StringComparison.Ordinal);
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
}
