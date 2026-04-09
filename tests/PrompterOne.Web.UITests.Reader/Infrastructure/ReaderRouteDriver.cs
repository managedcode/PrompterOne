using System.Runtime.CompilerServices;
using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Web.UITests;

internal static class ReaderRouteDriver
{
    internal static Task OpenLearnAsync(
        IPage page,
        string route,
        [CallerMemberName] string scenarioName = "") =>
        OpenAsync(page, route, UiTestIds.Learn.Page, scenarioName);

    internal static Task OpenTeleprompterAsync(
        IPage page,
        string route,
        [CallerMemberName] string scenarioName = "") =>
        OpenAsync(page, route, UiTestIds.Teleprompter.Page, scenarioName);

    internal static Task OpenSettingsAsync(
        IPage page,
        [CallerMemberName] string scenarioName = "") =>
        OpenAsync(page, BrowserTestConstants.Routes.Settings, UiTestIds.Settings.Page, scenarioName);

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
