using PrompterLive.Core.Models.Documents;

namespace PrompterLive.Core.Abstractions;

public interface IScriptRepository
{
    Task InitializeAsync(IEnumerable<StoredScriptDocument> seedDocuments, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredScriptSummary>> ListAsync(CancellationToken cancellationToken = default);

    Task<StoredScriptDocument?> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<StoredScriptDocument> SaveAsync(
        string title,
        string text,
        string? documentName = null,
        string? existingId = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
