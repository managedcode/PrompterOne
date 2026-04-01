using System.Text.Json;
using Microsoft.JSInterop;
using PrompterLive.Core.Abstractions;
using PrompterLive.Core.Models.Documents;

namespace PrompterLive.Shared.Services;

public sealed class BrowserScriptRepository(IJSRuntime jsRuntime) : IScriptRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private IReadOnlyList<BrowserStoredScriptDocumentDto> _seedDocuments = [];

    public Task InitializeAsync(IEnumerable<StoredScriptDocument> initialDocuments, CancellationToken cancellationToken = default)
    {
        _seedDocuments = initialDocuments
            .Select(ToDto)
            .ToList();

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<StoredScriptSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        var documents = await LoadDocumentsAsync(cancellationToken);

        return documents
            .Select(ToDocument)
            .OrderByDescending(document => document.UpdatedAt)
            .Select(document => new StoredScriptSummary(
                document.Id,
                document.Title,
                document.DocumentName,
                document.UpdatedAt,
                CountWords(document.Text),
                document.FolderId))
            .ToList();
    }

    public async Task<StoredScriptDocument?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var dto = (await LoadDocumentsAsync(cancellationToken))
            .FirstOrDefault(document => string.Equals(document.Id, id, StringComparison.Ordinal));

        return dto is null ? null : ToDocument(dto);
    }

    public async Task<StoredScriptDocument> SaveAsync(
        string title,
        string text,
        string? documentName = null,
        string? existingId = null,
        string? folderId = null,
        CancellationToken cancellationToken = default)
    {
        title = string.IsNullOrWhiteSpace(title) ? "Untitled Script" : title.Trim();
        documentName = string.IsNullOrWhiteSpace(documentName)
            ? $"{BrowserStorageSlugifier.Slugify(title)}.tps"
            : documentName;

        var persistedFolderId = await ResolveFolderIdAsync(existingId, folderId, cancellationToken);
        var document = new StoredScriptDocument(
            Id: string.IsNullOrWhiteSpace(existingId) ? BrowserStorageSlugifier.Slugify(Path.GetFileNameWithoutExtension(documentName)) : existingId,
            Title: title,
            Text: text ?? string.Empty,
            DocumentName: documentName,
            UpdatedAt: DateTimeOffset.UtcNow,
            FolderId: persistedFolderId);

        var documents = await LoadMutableDocumentsAsync(cancellationToken);
        UpsertDocument(documents, ToDto(document));
        await SaveStoredDocumentsAsync(documents, cancellationToken);
        return document;
    }

    public async Task MoveToFolderAsync(string id, string? folderId, CancellationToken cancellationToken = default)
    {
        var documents = await LoadMutableDocumentsAsync(cancellationToken);
        var index = documents.FindIndex(document => string.Equals(document.Id, id, StringComparison.Ordinal));
        if (index < 0)
        {
            return;
        }

        documents[index].FolderId = string.IsNullOrWhiteSpace(folderId) ? null : folderId;
        documents[index].UpdatedAt = DateTimeOffset.UtcNow;

        await SaveStoredDocumentsAsync(documents, cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var documents = await LoadMutableDocumentsAsync(cancellationToken);
        if (documents.RemoveAll(document => string.Equals(document.Id, id, StringComparison.Ordinal)) == 0)
        {
            return;
        }

        await SaveStoredDocumentsAsync(documents, cancellationToken);
    }

    private async Task<string?> ResolveFolderIdAsync(
        string? existingId,
        string? folderId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(folderId))
        {
            return folderId;
        }

        if (string.IsNullOrWhiteSpace(existingId))
        {
            return null;
        }

        var existingDocument = await GetAsync(existingId, cancellationToken);
        return existingDocument?.FolderId;
    }

    private async Task<List<BrowserStoredScriptDocumentDto>> LoadDocumentsAsync(CancellationToken cancellationToken)
    {
        var storedDocuments = await LoadStoredDocumentsAsync(cancellationToken);
        if (await IsMaterializedAsync(cancellationToken))
        {
            return storedDocuments;
        }

        return MergeDocuments(
            _seedDocuments,
            storedDocuments.Where(document => !LegacyLibrarySeedCatalog.IsLegacyDocument(document)));
    }

    private async Task<List<BrowserStoredScriptDocumentDto>> LoadMutableDocumentsAsync(CancellationToken cancellationToken)
    {
        var storedDocuments = await LoadStoredDocumentsAsync(cancellationToken);
        if (await IsMaterializedAsync(cancellationToken))
        {
            return storedDocuments;
        }

        var materializedDocuments = MergeDocuments(
            _seedDocuments,
            storedDocuments.Where(document => !LegacyLibrarySeedCatalog.IsLegacyDocument(document)));

        await SaveStoredDocumentsAsync(materializedDocuments, cancellationToken);
        return materializedDocuments;
    }

    private async Task<List<BrowserStoredScriptDocumentDto>> LoadStoredDocumentsAsync(CancellationToken cancellationToken)
    {
        var json = await _jsRuntime.InvokeAsync<string?>(
            BrowserStorageMethodNames.LoadStorageValue,
            cancellationToken,
            BrowserStorageKeys.DocumentLibrary);

        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<BrowserStoredScriptDocumentDto>>(json, JsonOptions) ?? [];
    }

    private async Task SaveStoredDocumentsAsync(
        IReadOnlyCollection<BrowserStoredScriptDocumentDto> documents,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(documents, JsonOptions);
        await _jsRuntime.InvokeVoidAsync(
            BrowserStorageMethodNames.SaveStorageValue,
            cancellationToken,
            BrowserStorageKeys.DocumentLibrary,
            json);

        await _jsRuntime.InvokeVoidAsync(
            BrowserStorageMethodNames.SaveStorageValue,
            cancellationToken,
            BrowserStorageKeys.DocumentSeedVersion,
            BrowserStorageKeys.LibraryMaterializationVersion);
    }

    private async Task<bool> IsMaterializedAsync(CancellationToken cancellationToken)
    {
        var version = await _jsRuntime.InvokeAsync<string?>(
            BrowserStorageMethodNames.LoadStorageValue,
            cancellationToken,
            BrowserStorageKeys.DocumentSeedVersion);

        return string.Equals(version, BrowserStorageKeys.LibraryMaterializationVersion, StringComparison.Ordinal);
    }

    private static void UpsertDocument(
        List<BrowserStoredScriptDocumentDto> documents,
        BrowserStoredScriptDocumentDto document)
    {
        var index = documents.FindIndex(existing => string.Equals(existing.Id, document.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            documents[index] = document;
            return;
        }

        documents.Add(document);
    }

    private static int CountWords(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;

    private static StoredScriptDocument ToDocument(BrowserStoredScriptDocumentDto dto) =>
        new(
            dto.Id ?? string.Empty,
            dto.Title ?? "Untitled Script",
            dto.Text ?? string.Empty,
            dto.DocumentName ?? "untitled-script.tps",
            dto.UpdatedAt,
            string.IsNullOrWhiteSpace(dto.FolderId) ? null : dto.FolderId);

    private static BrowserStoredScriptDocumentDto ToDto(StoredScriptDocument document) =>
        new()
        {
            Id = document.Id,
            Title = document.Title,
            Text = document.Text,
            DocumentName = document.DocumentName,
            UpdatedAt = document.UpdatedAt,
            FolderId = document.FolderId
        };

    private static List<BrowserStoredScriptDocumentDto> MergeDocuments(params IEnumerable<BrowserStoredScriptDocumentDto>[] sources)
    {
        var documentsById = new Dictionary<string, BrowserStoredScriptDocumentDto>(StringComparer.Ordinal);

        foreach (var source in sources)
        {
            foreach (var document in source)
            {
                if (string.IsNullOrWhiteSpace(document.Id))
                {
                    continue;
                }

                documentsById[document.Id] = document;
            }
        }

        return documentsById.Values
            .OrderByDescending(document => document.UpdatedAt)
            .ToList();
    }
}
