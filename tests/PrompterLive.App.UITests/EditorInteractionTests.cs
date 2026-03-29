using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace PrompterLive.App.UITests;

[Collection(StandaloneAppCollection.Name)]
public sealed class EditorInteractionTests(StandaloneAppFixture fixture)
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task EditorScreen_ShowsFloatingBarAndAppliesFormattingToSelectedSourceText()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync("/editor?id=rsvp-tech-demo");
            await Expect(page.GetByTestId("editor-page")).ToBeVisibleAsync();
            await Expect(page.GetByTestId("editor-source-input")).ToBeVisibleAsync();

            await page.GetByTestId("editor-source-input").EvaluateAsync(
                """
                element => {
                    const text = element.value;
                    const target = "welcome";
                    const start = text.indexOf(target);
                    element.focus();
                    element.setSelectionRange(start, start + target.length);
                    element.dispatchEvent(new Event("select", { bubbles: true }));
                    element.dispatchEvent(new Event("keyup", { bubbles: true }));
                }
                """);

            await Expect(page.GetByTestId("editor-floating-bar")).ToBeVisibleAsync();
            await page.GetByTestId("editor-float-emphasis").ClickAsync();

            var value = await page.GetByTestId("editor-source-input").InputValueAsync();
            Assert.Contains("[emphasis]welcome[/emphasis]", value, StringComparison.Ordinal);
            await Expect(page.GetByTestId("editor-source-highlight")).ToContainTextAsync("[emphasis]welcome[/emphasis]");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_UndoAndRedoWorkFromToolbarAndKeyboard()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync("/editor?id=rsvp-tech-demo");
            await Expect(page.GetByTestId("editor-source-input")).ToBeVisibleAsync();

            var initialValue = await page.GetByTestId("editor-source-input").InputValueAsync();

            await page.GetByTestId("editor-source-input").EvaluateAsync(
                """
                element => {
                    const addition = "\n[pause:2s]";
                    element.focus();
                    element.setSelectionRange(element.value.length, element.value.length);
                    element.value = element.value + addition;
                    element.dispatchEvent(new Event("input", { bubbles: true }));
                }
                """);

            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(new Regex(@"\[pause:2s\]\s*$"));

            await page.GetByTestId("editor-undo").ClickAsync();
            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(initialValue);

            await page.GetByTestId("editor-redo").ClickAsync();
            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(new Regex(@"\[pause:2s\]\s*$"));

            await page.GetByTestId("editor-source-input").ClickAsync();
            await page.Keyboard.PressAsync("Meta+Z");
            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(initialValue);

            await page.Keyboard.PressAsync("Meta+Shift+Z");
            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(new Regex(@"\[pause:2s\]\s*$"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_FloatingToolbarShowsAiAndPersistsSelectionFormatting()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync("/editor?id=rsvp-tech-demo");
            await Expect(page.GetByTestId("editor-source-input")).ToBeVisibleAsync();

            await page.GetByTestId("editor-source-input").EvaluateAsync(
                """
                element => {
                    const text = element.value;
                    const target = "transformative moment";
                    const start = text.indexOf(target);
                    element.focus();
                    element.setSelectionRange(start, start + target.length);
                    element.dispatchEvent(new Event("select", { bubbles: true }));
                    element.dispatchEvent(new Event("keyup", { bubbles: true }));
                }
                """);

            await Expect(page.GetByTestId("editor-floating-bar")).ToBeVisibleAsync();
            await Expect(page.GetByTestId("editor-floating-ai")).ToBeVisibleAsync();

            await page.GetByTestId("editor-floating-slow").ClickAsync();
            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(
                new Regex(@"\[slow\]transformative moment\[/slow\]"));

            await page.ReloadAsync();
            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(
                new Regex(@"\[slow\]transformative moment\[/slow\]"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task EditorScreen_StructureInspectorEditsRewriteHeaders()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync("/editor?id=quantum-computing");

            await Expect(page.GetByTestId("editor-active-segment-name")).ToBeVisibleAsync();
            await page.GetByTestId("editor-active-segment-name").FillAsync("Introduction");
            await page.GetByTestId("editor-active-segment-wpm").FillAsync("280");
            await page.GetByTestId("editor-active-segment-emotion").SelectOptionAsync(new[] { "Neutral" });
            await page.GetByTestId("editor-active-segment-timing").FillAsync("0:00-0:00");
            await page.GetByTestId("editor-active-block-name").FillAsync("Overview Block");
            await page.GetByTestId("editor-active-block-wpm").FillAsync("280");
            await page.GetByTestId("editor-active-block-emotion").SelectOptionAsync(new[] { "Neutral" });

            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(
                new Regex(@"## \[Introduction\|280WPM\|neutral\|0:00-0:00\]"));
            await Expect(page.GetByTestId("editor-source-input")).ToHaveValueAsync(
                new Regex(@"### \[Overview Block\|280WPM\|neutral\]"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
