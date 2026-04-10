using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

internal static class SettingsCardDriver
{
    internal static Task EnsureExpandedAsync(IPage page, string cardTestId) =>
        EnsureExpandedAsync(page.GetByTestId(cardTestId));

    internal static async Task EnsureExpandedAsync(ILocator card)
    {
        await Expect(card).ToBeVisibleAsync(new()
        {
            Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs
        });

        if (!await IsExpandedAsync(card))
        {
            await UiInteractionDriver.ClickAndContinueAsync(card);
        }

        await Expect(card).ToHaveAttributeAsync(
            BrowserTestConstants.State.ExpandedAttribute,
            BrowserTestConstants.State.OpenValue);
    }

    private static async Task<bool> IsExpandedAsync(ILocator card)
    {
        var state = await card.GetAttributeAsync(BrowserTestConstants.State.ExpandedAttribute);
        return string.Equals(state, BrowserTestConstants.State.OpenValue, StringComparison.Ordinal);
    }
}
