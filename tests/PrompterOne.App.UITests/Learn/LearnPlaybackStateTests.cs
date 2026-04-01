using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class LearnPlaybackStateTests(StandaloneAppFixture fixture) : AppUiTestBase(fixture), IClassFixture<StandaloneAppFixture>
{
    private sealed class ToggleIconState
    {
        public bool PlayHidden { get; init; }

        public bool PauseHidden { get; init; }
    }

    [Fact]
    public Task LearnScreen_PlayToggle_SwapsVisibleIconWhenPlaybackChanges() =>
        RunPageAsync(async page =>
        {
            await page.GotoAsync(BrowserTestConstants.Routes.LearnDemo);
            await Expect(page.GetByTestId(UiTestIds.Learn.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });

            var beforeToggle = await ReadToggleIconStateAsync(page);

            await page.GetByTestId(UiTestIds.Learn.PlayToggle).ClickAsync();

            var afterToggle = await ReadToggleIconStateAsync(page);

            Assert.True(beforeToggle.PlayHidden ^ beforeToggle.PauseHidden, "Expected exactly one learn playback icon to be visible before toggling.");
            Assert.True(afterToggle.PlayHidden ^ afterToggle.PauseHidden, "Expected exactly one learn playback icon to be visible after toggling.");
            Assert.NotEqual(beforeToggle.PlayHidden, afterToggle.PlayHidden);
            Assert.NotEqual(beforeToggle.PauseHidden, afterToggle.PauseHidden);
        });

    private static Task<ToggleIconState> ReadToggleIconStateAsync(Microsoft.Playwright.IPage page) =>
        page.EvaluateAsync<ToggleIconState>(
            $$"""
            () => {
                const button = document.querySelector('[data-testid="{{UiTestIds.Learn.PlayToggle}}"]');
                const playIcon = button?.querySelector('[data-toggle-icon="play"]');
                const pauseIcon = button?.querySelector('[data-toggle-icon="pause"]');

                return {
                    playHidden: !!playIcon?.hidden,
                    pauseHidden: !!pauseIcon?.hidden
                };
            }
            """);
}
