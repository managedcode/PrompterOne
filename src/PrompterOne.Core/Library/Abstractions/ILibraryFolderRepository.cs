using PrompterOne.Core.Models.Library;

namespace PrompterOne.Core.Abstractions;

public interface ILibraryFolderRepository
{
    Task InitializeAsync(IEnumerable<StoredLibraryFolder> initialFolders, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredLibraryFolder>> ListAsync(CancellationToken cancellationToken = default);

    Task<StoredLibraryFolder> CreateAsync(
        string name,
        string? parentId = null,
        CancellationToken cancellationToken = default);
}
