using PrompterOne.Core.Models.Media;

namespace PrompterOne.Core.Abstractions;

public interface IMediaDeviceService
{
    Task<IReadOnlyList<MediaDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default);
}
