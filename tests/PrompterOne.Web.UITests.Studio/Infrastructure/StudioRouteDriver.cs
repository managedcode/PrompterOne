using System.Runtime.CompilerServices;
using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Web.UITests;

internal static class StudioRouteDriver
{
    internal static Task OpenLibraryAsync(
        IPage page,
        [CallerMemberName] string scenarioName = "") =>
        OpenAsync(page, BrowserTestConstants.Routes.Library, UiTestIds.Library.Page, scenarioName);

    internal static Task OpenSettingsAsync(
        IPage page,
        [CallerMemberName] string scenarioName = "") =>
        OpenAsync(page, BrowserTestConstants.Routes.Settings, UiTestIds.Settings.Page, scenarioName);

    internal static Task OpenGoLiveAsync(
        IPage page,
        [CallerMemberName] string scenarioName = "") =>
        OpenAsync(page, BrowserTestConstants.Routes.GoLiveDemo, UiTestIds.GoLive.Page, scenarioName);

    internal static Task OpenTeleprompterAsync(
        IPage page,
        [CallerMemberName] string scenarioName = "") =>
        OpenAsync(page, BrowserTestConstants.Routes.TeleprompterDemo, UiTestIds.Teleprompter.Page, scenarioName);

    private static Task OpenAsync(
        IPage page,
        string route,
        string pageTestId,
        string scenarioName) =>
        BrowserRouteDriver.OpenPageAsync(
            page,
            route,
            pageTestId,
            $"{scenarioName}-{pageTestId}");
}
