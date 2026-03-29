using Microsoft.AspNetCore.Mvc.Testing;

namespace PrompterLive.Web.E2E.Tests;

public sealed class WebHostSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebHostSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RootRoute_ServesTheBlazorShell()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var html = await client.GetStringAsync("/");

        Assert.Contains("_content/PrompterLive.Shared/app.css", html, StringComparison.Ordinal);
        Assert.Contains("_content/PrompterLive.Shared/prompterlive.js", html, StringComparison.Ordinal);
        Assert.Contains("_framework/blazor.web.js", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SharedScriptBundle_IsServedFromStaticAssets()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var script = await client.GetStringAsync("/_content/PrompterLive.Shared/prompterlive.js");

        Assert.Contains("window.PrompterLive", script, StringComparison.Ordinal);
        Assert.Contains("attachCamera", script, StringComparison.Ordinal);
        Assert.Contains("startAutoScroll", script, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeepLinkRoute_FallsBackToSharedHostPage()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var html = await client.GetStringAsync("/settings");

        Assert.Contains("_content/PrompterLive.Shared/app.css", html, StringComparison.Ordinal);
        Assert.Contains("blazor.web.js", html, StringComparison.Ordinal);
    }
}
