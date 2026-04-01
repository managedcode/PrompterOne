using PrompterOne.Core.Models.Media;

namespace PrompterOne.Core.Abstractions;

public interface IMediaPermissionService
{
    Task<MediaPermissionsState> QueryAsync(CancellationToken cancellationToken = default);

    Task<MediaPermissionsState> RequestAsync(CancellationToken cancellationToken = default);
}
