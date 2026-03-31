using PrompterLive.Core.Models.Documents;
using PrompterLive.Core.Models.Workspace;

namespace PrompterLive.Core.Abstractions;

public interface IScriptSessionService
{
    ScriptWorkspaceState State { get; }

    event EventHandler? StateChanged;

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task NewAsync(CancellationToken cancellationToken = default);

    Task OpenAsync(StoredScriptDocument document, CancellationToken cancellationToken = default);

    Task UpdateDraftAsync(
        string title,
        string text,
        string? documentName = null,
        string? scriptId = null,
        CancellationToken cancellationToken = default);

    Task<StoredScriptDocument> SaveAsync(CancellationToken cancellationToken = default);

    Task UpdateReaderSettingsAsync(ReaderSettings settings);

    Task UpdateLearnSettingsAsync(LearnSettings settings);
}
