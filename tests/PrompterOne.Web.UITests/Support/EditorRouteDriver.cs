using Microsoft.Playwright;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Web.UITests;

internal static class EditorRouteDriver
{
    internal static async Task OpenReadyAsync(IPage page, string route, string? failureLabel = null)
    {
        await BrowserRouteDriver.OpenPageAsync(page, route, UiTestIds.Editor.Page, failureLabel);
        await EditorMonacoDriver.WaitUntilReadyAsync(page);
    }
}
