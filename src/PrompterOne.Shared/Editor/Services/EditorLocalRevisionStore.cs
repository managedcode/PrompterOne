using System.Globalization;
using PrompterOne.Core.Abstractions;

namespace PrompterOne.Shared.Services.Editor;

public sealed class EditorLocalRevisionStore(IUserSettingsStore settingsStore)
{
    private const int MaximumRevisionCount = 12;

    private readonly IUserSettingsStore _settingsStore = settingsStore;

    public async Task<EditorLocalRevisionRecord?> GetAsync(
        string? scriptId,
        string? revisionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(revisionId))
        {
            return null;
        }

        return (await LoadAsync(scriptId, cancellationToken))
            .FirstOrDefault(revision => string.Equals(revision.Id, revisionId, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyList<EditorLocalRevisionRecord>> LoadAsync(
        string? scriptId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scriptId))
        {
            return [];
        }

        return await _settingsStore.LoadAsync<List<EditorLocalRevisionRecord>>(
                   BuildStorageKey(scriptId),
                   cancellationToken)
               ?? [];
    }

    public async Task<IReadOnlyList<EditorLocalRevisionRecord>> RecordAsync(
        EditorLocalRevisionDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(draft.ScriptId))
        {
            return [];
        }

        var revisions = (await LoadAsync(draft.ScriptId, cancellationToken)).ToList();
        if (HasMatchingLatestRevision(revisions, draft))
        {
            return revisions;
        }

        revisions.Insert(0, CreateRevision(draft));
        TrimOldRevisions(revisions);

        await _settingsStore.SaveAsync(
            BuildStorageKey(draft.ScriptId),
            revisions,
            cancellationToken);

        return revisions;
    }

    internal static string BuildStorageKey(string scriptId) =>
        string.Concat(BrowserStorageKeys.EditorLocalHistoryKeyPrefix, scriptId.Trim());

    private static EditorLocalRevisionRecord CreateRevision(EditorLocalRevisionDraft draft) =>
        new(
            CreateRevisionId(draft.SavedAt),
            draft.Title,
            draft.DocumentName,
            draft.PersistedText,
            draft.SavedAt);

    private static string CreateRevisionId(DateTimeOffset savedAt) =>
        savedAt.UtcTicks.ToString(CultureInfo.InvariantCulture);

    private static bool HasMatchingLatestRevision(
        IReadOnlyList<EditorLocalRevisionRecord> revisions,
        EditorLocalRevisionDraft draft)
    {
        if (revisions.Count == 0)
        {
            return false;
        }

        var latest = revisions[0];
        return string.Equals(latest.PersistedText, draft.PersistedText, StringComparison.Ordinal)
               && string.Equals(latest.Title, draft.Title, StringComparison.Ordinal)
               && string.Equals(latest.DocumentName, draft.DocumentName, StringComparison.Ordinal);
    }

    private static void TrimOldRevisions(List<EditorLocalRevisionRecord> revisions)
    {
        if (revisions.Count <= MaximumRevisionCount)
        {
            return;
        }

        revisions.RemoveRange(MaximumRevisionCount, revisions.Count - MaximumRevisionCount);
    }
}

public sealed record EditorLocalRevisionDraft(
    string ScriptId,
    string Title,
    string DocumentName,
    string PersistedText,
    DateTimeOffset SavedAt);

public sealed record EditorLocalRevisionRecord(
    string Id,
    string Title,
    string DocumentName,
    string PersistedText,
    DateTimeOffset SavedAt);
