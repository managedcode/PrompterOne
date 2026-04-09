using System.Globalization;
using System.Text;
using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Services.Editor;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

[ClassDataSource<StandaloneAppFixture>(Shared = SharedType.PerClass)]
public sealed class EditorAiScrollStabilityTests(StandaloneAppFixture fixture)
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Test]
    public async Task EditorScreen_AiAction_DoesNotJumpScrollPositionForVisibleSelection()
    {
        var page = await _fixture.NewPageAsync(additionalContext: true);

        try
        {
            await AiProviderTestSeeder.SeedConfiguredOpenAiAsync(page);
            var sourceText = BuildAiScrollJumpText();
            await EditorIsolatedDraftDriver.CreateDraftAsync(page, sourceText);
            await EditorMonacoDriver.ClickAsync(page);

            var targetRange = ResolveAiScrollJumpTargetRange(sourceText);
            await EditorMonacoDriver.SetSelectionAsync(
                page,
                targetRange.Start,
                targetRange.End);
            await EditorMonacoDriver.CenterSelectionLineAsync(page);
            await EditorMonacoDriver.WaitForSelectionScrollAsync(
                page,
                BrowserTestConstants.Editor.AiScrollJumpMinimumScrollTopPx,
                BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs);

            var before = await EditorMonacoDriver.GetStateAsync(page);
            await Assert.That(before.ScrollTop >= BrowserTestConstants.Editor.AiScrollJumpMinimumScrollTopPx).IsTrue().Because($"Expected the selection reveal to scroll Monaco before the AI action, but ScrollTop stayed at {before.ScrollTop}.");

            await Expect(page.GetByTestId(UiTestIds.Editor.Ai)).ToBeEnabledAsync();
            await page.GetByTestId(UiTestIds.Editor.Ai).ClickAsync();
            await WaitForAiMutationAndStableScrollAsync(page, before.ScrollTop);

            var after = await EditorMonacoDriver.GetStateAsync(page);
            var value = await EditorMonacoDriver.SourceInput(page).InputValueAsync();

            await Assert.That(value).Contains(BrowserTestConstants.Editor.SimplifiedMoment);
            await Assert.That(Math.Abs(after.ScrollTop - before.ScrollTop)).IsBetween(0, BrowserTestConstants.Editor.AiScrollJumpMaximumAllowedDeltaPx);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static string BuildAiScrollJumpText()
    {
        var builder = new StringBuilder();
        for (var index = 0; index < BrowserTestConstants.Editor.AiScrollJumpLineCount; index++)
        {
            if (index > 0)
            {
                builder.Append('\n');
            }

            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                BrowserTestConstants.Editor.AiScrollJumpLineTemplate,
                index + 1);
        }

        return builder.ToString();
    }

    private static AiScrollJumpRange ResolveAiScrollJumpTargetRange(string sourceText)
    {
        var linePrefix = string.Format(
            CultureInfo.InvariantCulture,
            "Line {0:D3}",
            BrowserTestConstants.Editor.AiScrollJumpTargetLineIndex + 1);
        var lineStart = sourceText.IndexOf(linePrefix, StringComparison.Ordinal);
        if (lineStart < 0)
        {
            throw new InvalidOperationException($"Unable to locate the target line prefix \"{linePrefix}\".");
        }

        var targetStart = sourceText.IndexOf(
            BrowserTestConstants.Editor.TransformativeMoment,
            lineStart,
            StringComparison.Ordinal);
        if (targetStart < 0)
        {
            throw new InvalidOperationException("Unable to locate the AI simplify target text in the generated editor draft.");
        }

        return new AiScrollJumpRange(
            targetStart,
            targetStart + BrowserTestConstants.Editor.TransformativeMoment.Length);
    }

    private static Task WaitForAiMutationAndStableScrollAsync(IPage page, double baselineScrollTop) =>
        page.WaitForFunctionAsync(
            """
            (args) => {
                const sourceInput = document.querySelector(`[data-test="${args.sourceInputTestId}"]`);
                const harness = window[args.harnessGlobalName];
                const state = harness?.getState(args.stageTestId);
                const hasValue =
                    sourceInput instanceof HTMLInputElement ||
                    sourceInput instanceof HTMLTextAreaElement;
                if (!hasValue || !state) {
                    return false;
                }

                const sourceValue = sourceInput.value ?? "";
                const scrollTop = typeof state.scrollTop === "number" ? state.scrollTop : 0;

                return sourceValue.includes(args.expectedText) &&
                    Math.abs(scrollTop - args.baselineScrollTop) <= args.maxAllowedDelta;
            }
            """,
            new
            {
                baselineScrollTop,
                expectedText = BrowserTestConstants.Editor.SimplifiedMoment,
                harnessGlobalName = EditorMonacoRuntimeContract.BrowserHarnessGlobalName,
                maxAllowedDelta = BrowserTestConstants.Editor.AiScrollJumpMaximumAllowedDeltaPx,
                sourceInputTestId = UiTestIds.Editor.SourceInput,
                stageTestId = UiTestIds.Editor.SourceStage
            },
            new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

    private readonly record struct AiScrollJumpRange(int Start, int End);
}
