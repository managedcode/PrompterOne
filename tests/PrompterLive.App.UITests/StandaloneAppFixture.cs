using System.Collections.Concurrent;
using System.Net;
using Microsoft.Playwright;

namespace PrompterLive.App.UITests;

public sealed partial class StandaloneAppFixture : IAsyncLifetime
{
    private const int BaseAddressPort = 5051;
    private const string BaseAddressValue = "http://localhost:5051";
    private const int CrossProcessMutexTimeoutSeconds = 300;
    private const string CrossProcessMutexName = "PrompterLive.App.UITests.Runtime.5051";
    private const int ServerStartupTimeoutSeconds = 60;
    private const int ServerProbeDelayMilliseconds = 500;
    private readonly ConcurrentBag<IBrowserContext> _contexts = [];
    private SharedRuntimeHandle? _runtimeHandle;

    public string BaseAddress => BaseAddressValue;
    public IPlaywright Playwright => _runtimeHandle?.Playwright ?? throw new InvalidOperationException("UI test runtime is not initialized.");
    public IBrowser Browser => _runtimeHandle?.Browser ?? throw new InvalidOperationException("UI test runtime is not initialized.");

    public async Task InitializeAsync()
    {
        _runtimeHandle = await SharedRuntime.AcquireAsync();
    }

    public async Task DisposeAsync()
    {
        while (_contexts.TryTake(out var context))
        {
            try
            {
                await context.DisposeAsync();
            }
            catch
            {
            }
        }

        if (_runtimeHandle is not null)
        {
            await SharedRuntime.ReleaseAsync();
            _runtimeHandle = null;
        }
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseAddress
        });
        await context.GrantPermissionsAsync(["camera", "microphone"], new BrowserContextGrantPermissionsOptions
        {
            Origin = BaseAddress
        });
        await ConfigureMediaHarnessAsync(context);
        _contexts.Add(context);
        return await context.NewPageAsync();
    }

    private static class SharedRuntime
    {
        private static readonly SemaphoreSlim LifecycleGate = new(1, 1);
        private static IBrowser? _browser;
        private static Mutex? _crossProcessMutex;
        private static bool _ownsCrossProcessMutex;
        private static IPlaywright? _playwright;
        private static StaticSpaServer? _server;

        public static async Task<SharedRuntimeHandle> AcquireAsync()
        {
            await LifecycleGate.WaitAsync();
            try
            {
                if (_server is null || _playwright is null || _browser is null)
                {
                    AcquireCrossProcessMutex();

                    try
                    {
                        await EnsureBaseAddressIsAvailableAsync();
                        _server = new StaticSpaServer(
                            GetAppWwwrootDirectory(),
                            GetFrameworkDirectory(),
                            GetSharedWwwrootDirectory(),
                            GetAppScopedStylesheetPath(),
                            GetSharedScopedStylesheetPath(),
                            GetHotReloadStaticAssetsDirectory(),
                            BaseAddressValue);
                        await _server.StartAsync();
                        await WaitForServerAsync();

                        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
                        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                        {
                            Headless = true,
                            Args =
                            [
                                "--use-fake-ui-for-media-stream",
                                "--use-fake-device-for-media-stream"
                            ]
                        });
                    }
                    catch
                    {
                        await DisposeRuntimeAsync();
                        ReleaseCrossProcessMutex();
                        throw;
                    }
                }

                return new SharedRuntimeHandle(_playwright!, _browser!);
            }
            finally
            {
                LifecycleGate.Release();
            }
        }

        public static Task ReleaseAsync() => Task.CompletedTask;

        private static void AcquireCrossProcessMutex()
        {
            if (_ownsCrossProcessMutex)
            {
                return;
            }

            var mutex = new Mutex(false, CrossProcessMutexName);

            try
            {
                try
                {
                    if (!mutex.WaitOne(TimeSpan.FromSeconds(CrossProcessMutexTimeoutSeconds)))
                    {
                        throw new TimeoutException(
                            $"PrompterLive.App.UITests could not acquire exclusive runtime access to {BaseAddressValue} within {CrossProcessMutexTimeoutSeconds} seconds.");
                    }
                }
                catch (AbandonedMutexException)
                {
                }

                _crossProcessMutex = mutex;
                _ownsCrossProcessMutex = true;
            }
            catch
            {
                mutex.Dispose();
                throw;
            }
        }

        private static void ReleaseCrossProcessMutex()
        {
            if (!_ownsCrossProcessMutex || _crossProcessMutex is null)
            {
                return;
            }

            try
            {
                _crossProcessMutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
            }
            finally
            {
                _crossProcessMutex.Dispose();
                _crossProcessMutex = null;
                _ownsCrossProcessMutex = false;
            }
        }

        private static async Task DisposeRuntimeAsync()
        {
            if (_browser is not null)
            {
                await _browser.DisposeAsync();
                _browser = null;
            }

            _playwright?.Dispose();
            _playwright = null;

            if (_server is not null)
            {
                await _server.DisposeAsync();
                _server = null;
            }
        }
    }

    private sealed record SharedRuntimeHandle(IPlaywright Playwright, IBrowser Browser);

    private static async Task WaitForServerAsync()
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(ServerStartupTimeoutSeconds);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await GetServerStateAsync() == ServerState.Valid)
            {
                return;
            }

            await Task.Delay(ServerProbeDelayMilliseconds);
        }

        throw new TimeoutException($"PrompterLive.App did not start listening on {BaseAddressValue} within the timeout.");
    }

    private static async Task<ServerState> GetServerStateAsync()
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
                return ServerState.NotRunning;
            }

            var body = await response.Content.ReadAsStringAsync();
            if (!body.Contains("Prompter.live", StringComparison.Ordinal))
            {
                return ServerState.Invalid;
            }

            var assetName = GetExpectedFrameworkAssetName();
            if (string.IsNullOrWhiteSpace(assetName))
            {
                return ServerState.Valid;
            }

            using var assetResponse = await client.GetAsync($"{BaseAddressValue}/_framework/{assetName}");
            return assetResponse.StatusCode is HttpStatusCode.OK
                ? ServerState.Valid
                : ServerState.Invalid;
        }
        catch
        {
            return ServerState.NotRunning;
        }
    }

    private static string? GetExpectedFrameworkAssetName()
    {
        var frameworkDirectory = GetFrameworkDirectory();

        if (!Directory.Exists(frameworkDirectory))
        {
            return null;
        }

        return Directory
            .EnumerateFiles(frameworkDirectory, "PrompterLive.App*.wasm", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .Select(Path.GetFileName)
            .FirstOrDefault();
    }

    private static async Task EnsureBaseAddressIsAvailableAsync()
    {
        var listenerPids = await GetListeningPidsAsync();

        var conflictingPids = listenerPids
            .Distinct()
            .ToArray();

        if (conflictingPids.Length > 0)
        {
            throw new InvalidOperationException(CreatePortConflictMessage(conflictingPids));
        }
    }

    private static string CreatePortConflictMessage(IReadOnlyCollection<int> conflictingPids) =>
        $"PrompterLive.App.UITests requires exclusive access to {BaseAddressValue}, but it is already in use by process ID(s): {string.Join(", ", conflictingPids.Order())}. Stop the other browser suite or manual host before running this test project.";

    private static string GetListeningProcessArguments() => $"-t -iTCP:{BaseAddressPort} -sTCP:LISTEN";

    private static async Task<int[]> GetListeningPidsAsync()
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "lsof",
                Arguments = GetListeningProcessArguments(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        if (!process.Start())
        {
            return Array.Empty<int>();
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return output
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var pid) ? pid : 0)
            .Where(pid => pid > 0)
            .ToArray();
    }

    private static string GetAppWwwrootDirectory() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/PrompterLive.App/wwwroot"));

    private static string GetFrameworkDirectory() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/PrompterLive.App/bin/Debug/net10.0/wwwroot/_framework"));

    private static string GetSharedWwwrootDirectory() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/PrompterLive.Shared/wwwroot"));

    private static string GetAppScopedStylesheetPath() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/PrompterLive.App/obj/Debug/net10.0/scopedcss/bundle/PrompterLive.App.styles.css"));

    private static string GetSharedScopedStylesheetPath() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/PrompterLive.Shared/obj/Debug/net10.0/scopedcss/projectbundle/PrompterLive.Shared.bundle.scp.css"));

    private static string? GetHotReloadStaticAssetsDirectory()
    {
        var packagesRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages",
            "microsoft.dotnet.hotreload.webassembly.browser");

        if (!Directory.Exists(packagesRoot))
        {
            return null;
        }

        return Directory
            .EnumerateDirectories(packagesRoot)
            .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => Path.Combine(path, "staticwebassets"))
            .FirstOrDefault(Directory.Exists);
    }

    private enum ServerState
    {
        NotRunning,
        Valid,
        Invalid
    }
}
