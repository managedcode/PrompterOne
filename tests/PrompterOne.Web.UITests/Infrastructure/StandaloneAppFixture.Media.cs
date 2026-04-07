using Microsoft.Playwright;

namespace PrompterOne.Web.UITests;

public sealed partial class StandaloneAppFixture
{
    private static async Task ConfigureMediaHarnessAsync(IBrowserContext context)
    {
        await context.AddInitScriptAsync(BrowserTestConstants.Media.RuntimeContractInitializationScript);
        await context.AddInitScriptAsync(scriptPath: GetMediaHarnessScriptPath());
    }

    private static string GetMediaHarnessScriptPath() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../tests/PrompterOne.Web.UITests/Media/synthetic-media-harness.js"));
}
