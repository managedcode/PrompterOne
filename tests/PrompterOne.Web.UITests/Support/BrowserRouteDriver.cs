using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PrompterOne.Testing;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.Web.UITests;

internal static class BrowserRouteDriver
{
    private const int RouteBootstrapAttemptCount = 2;
    private const string RouteFailurePrefix = "route-open";

    internal static async Task OpenPageAsync(
        IPage page,
        string route,
        string pageTestId,
        string? failureLabel = null)
    {
        if (await IsCurrentRouteReadyAsync(page, route, pageTestId))
        {
            return;
        }

        for (var attempt = 1; attempt <= RouteBootstrapAttemptCount; attempt++)
        {
            await page.GotoAsync(route, new() { WaitUntil = WaitUntilState.NetworkIdle });
            if (await IsPageVisibleAsync(page, pageTestId, BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs))
            {
                return;
            }

            if (attempt < RouteBootstrapAttemptCount && TestEnvironment.IsCiEnvironment)
            {
                await page.GotoAsync(UiTestHostConstants.BlankPagePath, new() { WaitUntil = WaitUntilState.NetworkIdle });
            }
        }

        await TryCaptureFailurePageAsync(page, failureLabel ?? $"{RouteFailurePrefix}-{pageTestId}");
        await Expect(page.GetByTestId(pageTestId)).ToBeVisibleAsync(
            new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
    }

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

    private static async Task<bool> IsCurrentRouteReadyAsync(IPage page, string route, string pageTestId)
    {
        if (!IsCurrentRoute(page, route))
        {
            return false;
        }

        return await IsPageVisibleAsync(page, pageTestId, BrowserTestConstants.Timing.DefaultVisibleTimeoutMs);
    }

    private static bool IsCurrentRoute(IPage page, string route)
    {
        if (!Uri.TryCreate(page.Url, UriKind.Absolute, out var pageUri))
        {
            return false;
        }

        return string.Equals(pageUri.PathAndQuery, route, StringComparison.Ordinal);
    }

    private static async Task<bool> IsPageVisibleAsync(IPage page, string pageTestId, int timeoutMs)
    {
        try
        {
            await page.GetByTestId(pageTestId).WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            return true;
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }

    private static async Task TryCaptureFailurePageAsync(IPage page, string failureLabel)
    {
        if (page.IsClosed)
        {
            return;
        }

        try
        {
            await UiScenarioArtifacts.CaptureFailurePageAsync(page, failureLabel);
        }
        catch
        {
        }
    }
}
