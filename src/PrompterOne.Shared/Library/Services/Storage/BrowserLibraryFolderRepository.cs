using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using PrompterOne.Core.Abstractions;
using PrompterOne.Core.Models.Library;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Services;

public sealed class BrowserLibraryFolderRepository(IJSRuntime jsRuntime, IStringLocalizer<SharedResource> localizer) : ILibraryFolderRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;
    private IReadOnlyList<BrowserStoredLibraryFolderDto> _seedFolders = [];

    public Task InitializeAsync(IEnumerable<StoredLibraryFolder> initialFolders, CancellationToken cancellationToken = default)
    {
        _seedFolders = initialFolders
            .Select(ToDto)
            .ToList();

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<StoredLibraryFolder>> ListAsync(CancellationToken cancellationToken = default)
    {
        var folders = await LoadFoldersAsync(cancellationToken);

        return folders
            .Select(MapFolder)
            .OrderBy(folder => folder.DisplayOrder)
            .ThenBy(folder => folder.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<StoredLibraryFolder> CreateAsync(
        string name,
        string? parentId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        if (normalizedName.Length == 0)
        {
            throw new InvalidOperationException(Text(UiTextKey.LibraryFolderNameRequiredMessage));
        }

        var folders = await ListAsync(cancellationToken);
        var folder = new StoredLibraryFolder(
            Id: BuildUniqueId(normalizedName, folders),
            Name: normalizedName,
            ParentId: string.IsNullOrWhiteSpace(parentId) ? null : parentId,
            DisplayOrder: ResolveDisplayOrder(parentId, folders),
            UpdatedAt: DateTimeOffset.UtcNow);

        var mutableFolders = await LoadMutableFoldersAsync(cancellationToken);
        UpsertFolder(mutableFolders, ToDto(folder));
        await SaveStoredFoldersAsync(mutableFolders, cancellationToken);
        return folder;
    }

    private async Task<List<BrowserStoredLibraryFolderDto>> LoadFoldersAsync(CancellationToken cancellationToken)
    {
        var storedFolders = await LoadStoredFoldersAsync(cancellationToken);
        if (await IsMaterializedAsync(cancellationToken))
        {
            return storedFolders;
        }

        return MergeFolders(
            _seedFolders,
            storedFolders.Where(folder => !LegacyLibrarySeedCatalog.IsLegacyFolder(folder)));
    }

    private async Task<List<BrowserStoredLibraryFolderDto>> LoadMutableFoldersAsync(CancellationToken cancellationToken)
    {
        var storedFolders = await LoadStoredFoldersAsync(cancellationToken);
        if (await IsMaterializedAsync(cancellationToken))
        {
            return storedFolders;
        }

        var materializedFolders = MergeFolders(
            _seedFolders,
            storedFolders.Where(folder => !LegacyLibrarySeedCatalog.IsLegacyFolder(folder)));

        await SaveStoredFoldersAsync(materializedFolders, cancellationToken);
        return materializedFolders;
    }

    private async Task<List<BrowserStoredLibraryFolderDto>> LoadStoredFoldersAsync(CancellationToken cancellationToken)
    {
        var json = await _jsRuntime.InvokeAsync<string?>(
            BrowserStorageMethodNames.LoadStorageValue,
            cancellationToken,
            BrowserStorageKeys.FolderLibrary);

        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<BrowserStoredLibraryFolderDto>>(json, JsonOptions) ?? [];
    }

    private async Task SaveStoredFoldersAsync(
        IReadOnlyCollection<BrowserStoredLibraryFolderDto> folders,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(folders, JsonOptions);
        await _jsRuntime.InvokeVoidAsync(
            BrowserStorageMethodNames.SaveStorageValue,
            cancellationToken,
            BrowserStorageKeys.FolderLibrary,
            json);

        await _jsRuntime.InvokeVoidAsync(
            BrowserStorageMethodNames.SaveStorageValue,
            cancellationToken,
            BrowserStorageKeys.FolderSeedVersion,
            BrowserStorageKeys.LibraryMaterializationVersion);
    }

    private async Task<bool> IsMaterializedAsync(CancellationToken cancellationToken)
    {
        var version = await _jsRuntime.InvokeAsync<string?>(
            BrowserStorageMethodNames.LoadStorageValue,
            cancellationToken,
            BrowserStorageKeys.FolderSeedVersion);

        return string.Equals(version, BrowserStorageKeys.LibraryMaterializationVersion, StringComparison.Ordinal);
    }

    private static BrowserStoredLibraryFolderDto ToDto(StoredLibraryFolder folder) =>
        new()
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentId = folder.ParentId,
            DisplayOrder = folder.DisplayOrder,
            UpdatedAt = folder.UpdatedAt
        };

    private StoredLibraryFolder MapFolder(BrowserStoredLibraryFolderDto dto) =>
        new(
            Id: dto.Id ?? string.Empty,
            Name: dto.Name ?? Text(UiTextKey.LibraryUntitledFolderTitle),
            ParentId: string.IsNullOrWhiteSpace(dto.ParentId) ? null : dto.ParentId,
            DisplayOrder: dto.DisplayOrder,
            UpdatedAt: dto.UpdatedAt);

    private string Text(UiTextKey key) => _localizer[key.ToString()];

    private static int ResolveDisplayOrder(string? parentId, IEnumerable<StoredLibraryFolder> folders) =>
        folders
            .Where(folder => string.Equals(folder.ParentId, parentId, StringComparison.Ordinal))
            .Select(folder => folder.DisplayOrder)
            .DefaultIfEmpty(-1)
            .Max() + 1;

    private static string BuildUniqueId(string name, IEnumerable<StoredLibraryFolder> folders)
    {
        var usedIds = folders
            .Select(folder => folder.Id)
            .ToHashSet(StringComparer.Ordinal);
        var baseId = BrowserStorageSlugifier.Slugify(name);

        if (!usedIds.Contains(baseId))
        {
            return baseId;
        }

        var suffix = 2;
        var candidate = baseId;
        while (usedIds.Contains(candidate))
        {
            candidate = $"{baseId}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static void UpsertFolder(
        List<BrowserStoredLibraryFolderDto> folders,
        BrowserStoredLibraryFolderDto folder)
    {
        var index = folders.FindIndex(existing => string.Equals(existing.Id, folder.Id, StringComparison.Ordinal));
        if (index >= 0)
        {
            folders[index] = folder;
            return;
        }

        folders.Add(folder);
    }

    private static List<BrowserStoredLibraryFolderDto> MergeFolders(params IEnumerable<BrowserStoredLibraryFolderDto>[] sources)
    {
        var foldersById = new Dictionary<string, BrowserStoredLibraryFolderDto>(StringComparer.Ordinal);

        foreach (var source in sources)
        {
            foreach (var folder in source)
            {
                if (string.IsNullOrWhiteSpace(folder.Id))
                {
                    continue;
                }

                foldersById[folder.Id] = folder;
            }
        }

        return foldersById.Values
            .OrderBy(folder => folder.DisplayOrder)
            .ThenBy(folder => folder.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
