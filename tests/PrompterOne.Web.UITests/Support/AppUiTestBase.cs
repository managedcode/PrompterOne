using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace PrompterOne.Web.UITests;

public abstract class AppUiTestBase(StandaloneAppFixture fixture)
{
    private readonly StandaloneAppFixture _fixture = fixture;

    protected Task RunPageAsync(
        Func<IPage, Task> scenario,
        [CallerMemberName] string testName = "") =>
        RunPageAsync(async page =>
        {
            await scenario(page);
            return true;
        }, testName);

    protected async Task<T> RunPageAsync<T>(
        Func<IPage, Task<T>> scenario,
        [CallerMemberName] string testName = "")
    {
        var page = await _fixture.NewPageAsync(additionalContext: true, contextKey: testName);

        try
        {
            return await scenario(page);
        }
        catch
        {
            await TryCaptureFailurePageAsync(page, testName);
            throw;
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    private static async Task TryCaptureFailurePageAsync(IPage page, string testName)
    {
        try
        {
            await UiScenarioArtifacts.CaptureFailurePageAsync(page, testName);
        }
        catch
        {
        }
    }
}
