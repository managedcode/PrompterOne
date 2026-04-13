using Microsoft.Playwright;
using PrompterOne.Shared.Components.Editor;
using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

[System.Obsolete]

[ClassDataSource<StandaloneAppFixture>(Shared = SharedType.PerClass)]
public sealed class EditorToolbarCoverageTests(StandaloneAppFixture fixture)
{
    private readonly StandaloneAppFixture _fixture = fixture;

    public static IEnumerable<EditorMenuScenario> MenuScenarios => EditorToolbarCoverageScenarios.MenuScenarios;

    public static IEnumerable<EditorMenuScenario> FloatingMenuScenarios => EditorToolbarCoverageScenarios.FloatingMenuScenarios;

    public static IEnumerable<EditorCommandScenario> ToolbarCommandScenarios => EditorToolbarCoverageScenarios.ToolbarCommandScenarios;

    public static IEnumerable<EditorCommandScenario> FloatingCommandScenarios => EditorToolbarCoverageScenarios.FloatingCommandScenarios;

    [Test]
    public async Task EditorToolbar_MenuTriggers_ExposeExpectedBehavior()
    {
        await UiPageScenarioDriver.RunWithIsolatedPageRetryAsync(
            _fixture,
            async page =>
            {
                await OpenEditorAsync(page);

                foreach (var scenario in MenuScenarios)
                {
                    await RunCoverageStepAsync(
                        scenario.TriggerTestId,
                        async () =>
                        {
                            await UiInteractionDriver.ClickAndWaitForVisibleAsync(
                                page.GetByTestId(scenario.TriggerTestId),
                                page.GetByTestId(scenario.PanelTestId));
                            await page.Keyboard.PressAsync("Escape");
                        });
                }
            },
            "Toolbar menu coverage failed.");
    }

    [Test]
    public async Task EditorToolbar_FloatingMenuTriggers_ExposeExpectedBehavior()
    {
        await UiPageScenarioDriver.RunWithIsolatedPageRetryAsync(
            _fixture,
            async page =>
            {
                await OpenEditorAsync(page);

                foreach (var scenario in FloatingMenuScenarios)
                {
                    await RunCoverageStepAsync(
                        scenario.TriggerTestId,
                        async () =>
                        {
                            await SetSourceTextAndSelectPhraseAsync(page, BrowserTestSource.AlphaSource, BrowserTestSource.AlphaToken);
                            await Expect(page.GetByTestId(UiTestIds.Editor.FloatingBar))
                                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.FastVisibleTimeoutMs });
                            await UiInteractionDriver.ClickAndWaitForVisibleAsync(
                                page.GetByTestId(scenario.TriggerTestId),
                                page.GetByTestId(scenario.PanelTestId),
                                noWaitAfter: true);
                            await page.Keyboard.PressAsync("Escape");
                        });
                }
            },
            "Floating toolbar menu coverage failed.");
    }

    [Test]
    public async Task EditorToolbar_CommandButtons_MutateSource()
    {
        await UiPageScenarioDriver.RunWithIsolatedPageRetryAsync(
            _fixture,
            async page =>
            {
                await OpenEditorAsync(page);

                foreach (var scenario in ToolbarCommandScenarios)
                {
                    await RunCoverageStepAsync(
                        scenario.TestId,
                        async () =>
                        {
                            await PrepareScenarioAsync(page, scenario);
                            if (!string.IsNullOrWhiteSpace(scenario.MenuTriggerTestId))
                            {
                                await OpenScenarioMenuAsync(page, scenario);
                            }

                            var beforeValue = await EditorMonacoDriver.SourceInput(page).InputValueAsync();
                            await UiInteractionDriver.ClickAndContinueAsync(page.GetByTestId(scenario.TestId));
                            var afterValue = await EditorMonacoDriver.SourceInput(page).InputValueAsync();

                            await AssertCommandMutation(scenario, beforeValue, afterValue);
                        });
                }
            },
            "Toolbar command coverage failed.");
    }

    [Test]
    public async Task EditorToolbar_FloatingCommandButtons_MutateSource()
    {
        await UiPageScenarioDriver.RunWithIsolatedPageRetryAsync(
            _fixture,
            async page =>
            {
                await OpenEditorAsync(page);

                foreach (var scenario in FloatingCommandScenarios)
                {
                    await RunCoverageStepAsync(
                        scenario.TestId,
                        async () =>
                        {
                            await PrepareScenarioAsync(page, scenario);
                            await Expect(page.GetByTestId(UiTestIds.Editor.FloatingBar))
                                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.FastVisibleTimeoutMs });
                            if (!string.IsNullOrWhiteSpace(scenario.MenuTriggerTestId))
                            {
                                await OpenScenarioMenuAsync(page, scenario, noWaitAfter: true);
                            }

                            var beforeValue = await EditorMonacoDriver.SourceInput(page).InputValueAsync();
                            await UiInteractionDriver.ClickAndContinueAsync(page.GetByTestId(scenario.TestId), noWaitAfter: true);
                            var afterValue = await EditorMonacoDriver.SourceInput(page).InputValueAsync();

                            await AssertCommandMutation(scenario, beforeValue, afterValue);
                        });
                }
            },
            "Floating toolbar coverage failed.");
    }

    private static async Task RunCoverageStepAsync(string scenarioId, Func<Task> step)
    {
        try
        {
            await step();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Editor toolbar coverage step failed for '{scenarioId}'.", exception);
        }
    }

    private static async Task OpenScenarioMenuAsync(IPage page, EditorCommandScenario scenario, bool noWaitAfter = false)
    {
        if (string.IsNullOrWhiteSpace(scenario.MenuTriggerTestId))
        {
            return;
        }

        var trigger = page.GetByTestId(scenario.MenuTriggerTestId);
        if (string.IsNullOrWhiteSpace(scenario.MenuPanelTestId))
        {
            await UiInteractionDriver.ClickAndContinueAsync(trigger, noWaitAfter);
            return;
        }

        await UiInteractionDriver.ClickAndWaitForVisibleAsync(
            trigger,
            page.GetByTestId(scenario.MenuPanelTestId),
            noWaitAfter);
    }

    private static async Task AssertCommandMutation(EditorCommandScenario scenario, string beforeValue, string afterValue)
    {
        await Assert.That(afterValue).IsNotEqualTo(beforeValue);

        switch (scenario.Command.Kind)
        {
            case EditorCommandKind.Wrap:
            {
                var expectedFragment = string.Concat(
                    scenario.Command.PrimaryToken,
                    BrowserTestSource.AlphaToken,
                    scenario.Command.SecondaryToken);
                await Assert.That(afterValue).Contains(expectedFragment);
                break;
            }
            case EditorCommandKind.ClearColor:
                await Assert.That(afterValue).DoesNotContain(BrowserTestSource.ColoredAlphaToken);
                await Assert.That(afterValue).Contains(BrowserTestSource.AlphaToken);
                break;
            default:
            {
                var normalizedPrimaryToken = scenario.Command.PrimaryToken.TrimEnd('\r', '\n');
                await Assert.That(afterValue).Contains(normalizedPrimaryToken);
                break;
            }
        }
    }

    private static async Task OpenEditorAsync(IPage page)
    {
        await EditorIsolatedDraftDriver.CreateSeededDraftAsync(page, BrowserTestConstants.Scripts.DemoId);
    }

    private static Task PrepareScenarioAsync(IPage page, EditorCommandScenario scenario) =>
        scenario.SelectionMode switch
        {
            EditorScenarioSelectionMode.WrapSelection => SetSourceTextAndSelectAlphaAsync(page, BrowserTestSource.AlphaSource),
            EditorScenarioSelectionMode.ClearColorSelection => SetSourceTextAndSelectColoredAlphaAsync(page),
            _ => SetSourceTextAndSetCaretAtEndAsync(page, BrowserTestSource.AlphaSource)
        };

    private static Task SetSourceTextAndSelectAlphaAsync(IPage page, string text) =>
        SetSourceTextAndSelectPhraseAsync(page, text, BrowserTestSource.AlphaToken);

    private static async Task SetSourceTextAndSelectPhraseAsync(IPage page, string text, string targetPhrase)
    {
        await EditorMonacoDriver.SetTextAsync(page, text);
        await EditorMonacoDriver.SetSelectionByTextAsync(page, targetPhrase);
    }

    private static Task SetSourceTextAndSelectColoredAlphaAsync(IPage page) =>
        SetSourceTextAndSelectPhraseAsync(page, BrowserTestSource.ColoredSource, BrowserTestSource.ColoredAlphaSelectionTarget);

    private static async Task SetSourceTextAndSetCaretAtEndAsync(IPage page, string text)
    {
        await EditorMonacoDriver.SetTextAsync(page, text);
        await EditorMonacoDriver.SetCaretAtEndAsync(page);
    }

    private static class BrowserTestSource
    {
        internal const string AlphaToken = "Alpha";
        internal const string ColoredAlphaToken = "[loud]Alpha[/loud]";
        internal const string ColoredAlphaSelectionTarget = "Alpha";
        internal const string AlphaSource = """
            ## [Intro|140WPM|warm]
            ### [Opening Block|140WPM]
            Alpha
            """;
        internal const string ColoredSource = """
            ## [Intro|140WPM|warm]
            ### [Opening Block|140WPM]
            [loud]Alpha[/loud]
            """;
    }
}
