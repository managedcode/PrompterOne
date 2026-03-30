using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace PrompterLive.App.UITests;

internal sealed class StaticSpaServer(
    string appWwwrootDirectory,
    string frameworkDirectory,
    string sharedWwwrootDirectory,
    string appScopedStylesheetPath,
    string sharedScopedStylesheetPath,
    string? hotReloadStaticAssetsDirectory,
    string requestedBaseAddress) : IAsyncDisposable
{
    private readonly string _appWwwrootDirectory = appWwwrootDirectory;
    private readonly string _appScopedStylesheetPath = appScopedStylesheetPath;
    private readonly string _frameworkDirectory = frameworkDirectory;
    private readonly string _sharedScopedStylesheetPath = sharedScopedStylesheetPath;
    private readonly string _sharedWwwrootDirectory = sharedWwwrootDirectory;
    private readonly string? _hotReloadStaticAssetsDirectory = hotReloadStaticAssetsDirectory;
    private readonly string _requestedBaseAddress = requestedBaseAddress;
    private WebApplication? _app;
    public string BaseAddress { get; private set; } = string.Empty;

    public async Task StartAsync()
    {
        ValidateStaticAssets();

        var app = BuildApplication();
        _app = app;
        await _app.StartAsync();
        BaseAddress = ResolveBaseAddress(_app);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        BaseAddress = string.Empty;
    }

    private void ValidateStaticAssets()
    {
        EnsureDirectoryExists(_appWwwrootDirectory, "App wwwroot directory");
        EnsureDirectoryExists(_frameworkDirectory, "Blazor framework directory");
        EnsureDirectoryExists(_sharedWwwrootDirectory, "Shared wwwroot directory");
        EnsureFileExists(_appScopedStylesheetPath, "App scoped stylesheet");
        EnsureFileExists(_sharedScopedStylesheetPath, "Shared scoped stylesheet");
    }

    private WebApplication BuildApplication()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(_requestedBaseAddress);

        var provider = BuildContentTypeProvider();
        var app = builder.Build();
        MapApplicationAssets(app, provider);
        return app;
    }

    private void MapApplicationAssets(WebApplication app, FileExtensionContentTypeProvider provider)
    {
        MapStaticDirectory(app, "/_framework", _frameworkDirectory, provider);
        MapSharedStaticDirectory(app, provider, _sharedWwwrootDirectory, _sharedScopedStylesheetPath);

        if (!string.IsNullOrWhiteSpace(_hotReloadStaticAssetsDirectory) && Directory.Exists(_hotReloadStaticAssetsDirectory))
        {
            MapStaticDirectory(app, "/_content/Microsoft.DotNet.HotReload.WebAssembly.Browser", _hotReloadStaticAssetsDirectory, provider);
        }

        app.MapGet($"/{Path.GetFileName(_appScopedStylesheetPath)}", context =>
            SendFileIfPresentAsync(context, _appScopedStylesheetPath, provider));
        app.MapGet("/{**path}", context => ServeAppAssetAsync(context, provider));
    }

    private static void EnsureDirectoryExists(string path, string description)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"{description} not found: {path}");
        }
    }

    private static void EnsureFileExists(string path, string description)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"{description} not found: {path}");
        }
    }

    private async Task ServeAppAssetAsync(HttpContext context, FileExtensionContentTypeProvider provider)
    {
        var requestPath = context.Request.Path.Value;
        if (!string.IsNullOrWhiteSpace(requestPath) && Path.HasExtension(requestPath))
        {
            var assetPath = requestPath.TrimStart('/');
            var physicalPath = Path.GetFullPath(Path.Combine(_appWwwrootDirectory, assetPath.Replace('/', Path.DirectorySeparatorChar)));
            if (!physicalPath.StartsWith(_appWwwrootDirectory, StringComparison.Ordinal) || !File.Exists(physicalPath))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await SendFileIfPresentAsync(context, physicalPath, provider);
            return;
        }

        await context.Response.SendFileAsync(Path.Combine(_appWwwrootDirectory, "index.html"));
    }

    private static string ResolveBaseAddress(WebApplication app)
    {
        var address = app.Urls.SingleOrDefault();
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException("Static SPA server did not expose a listening address after startup.");
        }

        var uri = new Uri(address, UriKind.Absolute);
        return uri.GetLeftPart(UriPartial.Authority);
    }

    private static FileExtensionContentTypeProvider BuildContentTypeProvider()
    {
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".dat"] = "application/octet-stream";
        provider.Mappings[".dll"] = "application/octet-stream";
        provider.Mappings[".gz"] = "application/gzip";
        provider.Mappings[".map"] = "application/json";
        provider.Mappings[".pdb"] = "application/octet-stream";
        provider.Mappings[".wasm"] = "application/wasm";
        provider.Mappings[".webmanifest"] = "application/manifest+json";
        return provider;
    }

    private static void MapSharedStaticDirectory(
        WebApplication app,
        FileExtensionContentTypeProvider contentTypeProvider,
        string sharedWwwrootDirectory,
        string sharedScopedStylesheetPath)
    {
        app.MapGet("/_content/PrompterLive.Shared/{**assetPath}", async context =>
        {
            var assetPath = context.Request.RouteValues["assetPath"]?.ToString();
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var sharedAssetPath = Path.GetFullPath(Path.Combine(sharedWwwrootDirectory, assetPath.Replace('/', Path.DirectorySeparatorChar)));
            if (sharedAssetPath.StartsWith(sharedWwwrootDirectory, StringComparison.Ordinal) && File.Exists(sharedAssetPath))
            {
                await SendFileIfPresentAsync(context, sharedAssetPath, contentTypeProvider);
                return;
            }

            if (assetPath.EndsWith(".bundle.scp.css", StringComparison.Ordinal))
            {
                await SendFileIfPresentAsync(context, sharedScopedStylesheetPath, contentTypeProvider);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
        });
    }

    private static void MapStaticDirectory(
        WebApplication app,
        string requestPrefix,
        string physicalDirectory,
        FileExtensionContentTypeProvider contentTypeProvider)
    {
        app.MapGet($"{requestPrefix}/{{**assetPath}}", async context =>
        {
            var assetPath = context.Request.RouteValues["assetPath"]?.ToString();
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var physicalPath = Path.GetFullPath(Path.Combine(physicalDirectory, assetPath.Replace('/', Path.DirectorySeparatorChar)));
            if (!physicalPath.StartsWith(physicalDirectory, StringComparison.Ordinal) || !File.Exists(physicalPath))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await SendFileIfPresentAsync(context, physicalPath, contentTypeProvider);
        });
    }

    private static async Task SendFileIfPresentAsync(
        HttpContext context,
        string physicalPath,
        FileExtensionContentTypeProvider contentTypeProvider)
    {
        if (!contentTypeProvider.TryGetContentType(physicalPath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        context.Response.ContentType = contentType;
        context.Response.Headers.CacheControl = "no-store";
        await context.Response.SendFileAsync(physicalPath);
    }
}
