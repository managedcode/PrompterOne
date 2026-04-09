using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

internal static class BrowserRouteDriver
{
    internal static Task WaitForRouteAsync(IPage page, string route)
    {
        var routePattern = new Regex($"{Regex.Escape(route)}$");

        return Expect(page).ToHaveURLAsync(
            routePattern,
            new()
            {
                Timeout = BrowserTestConstants.Timing.DefaultNavigationTimeoutMs
            });
    }
}
