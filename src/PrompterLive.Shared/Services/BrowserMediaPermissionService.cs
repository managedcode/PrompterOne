using Microsoft.JSInterop;
using PrompterLive.Core.Abstractions;
using PrompterLive.Core.Models.Media;

namespace PrompterLive.Shared.Services;

public sealed class BrowserMediaPermissionService : IMediaPermissionService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserMediaPermissionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<MediaPermissionsState> QueryAsync(CancellationToken cancellationToken = default)
    {
        return await _jsRuntime.InvokeAsync<MediaPermissionsState>("PrompterLive.media.queryPermissions", cancellationToken);
    }

    public async Task<MediaPermissionsState> RequestAsync(CancellationToken cancellationToken = default)
    {
        return await _jsRuntime.InvokeAsync<MediaPermissionsState>("PrompterLive.media.requestPermissions", cancellationToken);
    }
}
