using ManagedCode.Storage.VirtualFileSystem.Core;

namespace PrompterLive.Shared.Storage;

internal static class VfsDirectoryProvisioner
{
    public static async Task EnsureDirectoryAsync(
        IVirtualFileSystem fileSystem,
        VfsPath path,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileSystem);

        if (path.IsRoot)
        {
            return;
        }

        var directory = await fileSystem.GetDirectoryAsync(path, cancellationToken);
        if (await directory.ExistsAsync(cancellationToken))
        {
            return;
        }

        var parent = path.GetParent();
        await EnsureDirectoryAsync(fileSystem, parent, cancellationToken);

        var parentDirectory = await fileSystem.GetDirectoryAsync(parent, cancellationToken);
        _ = await parentDirectory.CreateDirectoryAsync(path.GetFileName(), cancellationToken);
    }
}
