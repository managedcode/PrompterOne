using PrompterLive.Core.Models.Media;

namespace PrompterLive.Core.Abstractions;

public interface IMediaPermissionService
{
    Task<MediaPermissionsState> QueryAsync(CancellationToken cancellationToken = default);

    Task<MediaPermissionsState> RequestAsync(CancellationToken cancellationToken = default);
}
