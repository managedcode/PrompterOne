using PrompterLive.Core.Models.Media;

namespace PrompterLive.Core.Abstractions;

public interface IMediaDeviceService
{
    Task<IReadOnlyList<MediaDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default);
}
