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
    private const string BaseAddressValue = "http://127.0.0.1:5187";
    private Process? _process;

    public string BaseAddress => BaseAddressValue;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _process = StartAppProcess();
        await WaitForServerAsync();

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
        var projectPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/PrompterLive.App/PrompterLive.App.csproj"));

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" --urls {BaseAddressValue}",
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
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);
        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync(BaseAddressValue);
                if (response.StatusCode is HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"PrompterLive.App did not start listening on {BaseAddressValue} within the timeout.");
    }
}
