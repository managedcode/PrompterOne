using Microsoft.Playwright;

namespace PrompterOne.Web.UITests;

public abstract class AppUiTestBase(StandaloneAppFixture fixture)
{
    private readonly StandaloneAppFixture _fixture = fixture;

    protected Task RunPageAsync(Func<IPage, Task> scenario) =>
        RunPageAsync(async page =>
        {
            await scenario(page);
            return true;
        });

    protected async Task<T> RunPageAsync<T>(Func<IPage, Task<T>> scenario)
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            return await scenario(page);
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
