using PrompterOne.Shared.Contracts;
using static Microsoft.Playwright.Assertions;

namespace PrompterOne.App.UITests;

public sealed class DynamicHostPortTests(StandaloneAppFixture fixture) : IClassFixture<StandaloneAppFixture>
{
    private readonly StandaloneAppFixture _fixture = fixture;

    [Fact]
    public async Task NewPageAsync_UsesDynamicLoopbackBaseAddress()
    {
        var baseAddress = new Uri(_fixture.BaseAddress);

        Assert.Equal(Uri.UriSchemeHttp, baseAddress.Scheme);
        Assert.True(baseAddress.IsLoopback);
        Assert.InRange(baseAddress.Port, UiTestHostConstants.MinimumDynamicPort, UiTestHostConstants.MaximumTcpPort);

        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(BrowserTestConstants.Routes.Library);
            await Expect(page.GetByTestId(UiTestIds.Library.Page))
                .ToBeVisibleAsync(new() { Timeout = BrowserTestConstants.Timing.ExtendedVisibleTimeoutMs });
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
}
