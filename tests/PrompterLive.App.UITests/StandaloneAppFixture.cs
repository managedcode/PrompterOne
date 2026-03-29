using System.Diagnostics;
using System.Net;
using Microsoft.Playwright;

namespace PrompterLive.App.UITests;

[CollectionDefinition(Name)]
public sealed class StandaloneAppCollection : ICollectionFixture<StandaloneAppFixture>
{
    public const string Name = "standalone-app";
}

public sealed class StandaloneAppFixture : IAsyncLifetime
{
    private const string BaseAddressValue = "http://localhost:5040";
    private Process? _process;

    public string BaseAddress => BaseAddressValue;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        if (!await IsServerAvailableAsync())
        {
            _process = StartAppProcess();
            await WaitForServerAsync();
        }

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        Playwright?.Dispose();

        if (_process is { HasExited: false })
        {
            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }
    }

    public async Task<IPage> NewPageAsync()
    {
        return await Browser.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = BaseAddress
        });
    }

    private static Process StartAppProcess()
    {
        var projectDirectory = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/PrompterLive.App"));

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run",
                WorkingDirectory = projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += static (_, _) => { };
        process.ErrorDataReceived += static (_, _) => { };

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start PrompterLive.App.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static async Task WaitForServerAsync()
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await IsServerAvailableAsync())
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"PrompterLive.App did not start listening on {BaseAddressValue} within the timeout.");
    }

    private static async Task<bool> IsServerAvailableAsync()
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        try
        {
            using var response = await client.GetAsync(BaseAddressValue);
            if (response.StatusCode is not HttpStatusCode.OK)
            {
                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            return body.Contains("Prompter.live", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}
