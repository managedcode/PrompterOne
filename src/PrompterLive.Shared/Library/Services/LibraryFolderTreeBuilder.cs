using PrompterLive.Core.Models.Library;
using PrompterLive.Shared.Components.Library;

namespace PrompterLive.Shared.Services.Library;

internal static class LibraryFolderTreeBuilder
{
    public static IReadOnlyList<LibraryFolderNodeViewModel> BuildTree(
        IReadOnlyList<StoredLibraryFolder> folders,
        IReadOnlyCollection<LibraryCardViewModel> cards,
        string selectedFolderId,
        ISet<string> expandedFolderIds)
    {
        var totalCountByFolder = BuildTotalCountByFolder(folders, cards);
        var childrenByParent = BuildChildrenLookup(folders);

        return BuildNodes(parentId: null, depth: 0).ToList();

        IReadOnlyList<LibraryFolderNodeViewModel> BuildNodes(string? parentId, int depth)
        {
            var children = childrenByParent[parentId].ToList();
            if (children.Count == 0)
            {
                return [];
            }

            return children
                .Select(folder => BuildNode(folder, depth))
                .ToList();
        }

        LibraryFolderNodeViewModel BuildNode(StoredLibraryFolder folder, int depth)
        {
            var childNodes = BuildNodes(folder.Id, depth + 1);
            var totalCount = totalCountByFolder.GetValueOrDefault(folder.Id);
            var hasSelectedDescendant = ContainsSelection(childNodes, selectedFolderId);
            var isExpanded = childNodes.Count > 0
                && (expandedFolderIds.Contains(folder.Id) || hasSelectedDescendant || depth == 0);

            return new LibraryFolderNodeViewModel(
                Id: folder.Id,
                Name: folder.Name,
                Depth: depth,
                TotalCount: totalCount,
                IsExpanded: isExpanded,
                IsSelected: string.Equals(folder.Id, selectedFolderId, StringComparison.Ordinal),
                ShowChevron: depth == 0,
                Children: childNodes);
        }
    }

    public static IReadOnlyList<LibraryFolderChipViewModel> BuildChips(
        IReadOnlyList<StoredLibraryFolder> folders,
        IReadOnlyCollection<LibraryCardViewModel> cards,
        string selectedFolderId)
    {
        var childrenByParent = BuildChildrenLookup(folders);
        var totalCountByFolder = BuildTotalCountByFolder(folders, cards);
        var foldersById = folders.ToDictionary(folder => folder.Id, StringComparer.Ordinal);
        var chipParentId = ResolveChipParentId(selectedFolderId, foldersById);

        return childrenByParent[chipParentId]
            .Select(folder => new LibraryFolderChipViewModel(
                Id: folder.Id,
                Name: folder.Name,
                TotalCount: totalCountByFolder.GetValueOrDefault(folder.Id),
                IsSelected: string.Equals(folder.Id, selectedFolderId, StringComparison.Ordinal)))
            .ToList();
    }

    public static IReadOnlyList<LibraryFolderOptionViewModel> BuildOptions(IReadOnlyList<StoredLibraryFolder> folders)
    {
        var childrenByParent = BuildChildrenLookup(folders);
        var options = new List<LibraryFolderOptionViewModel>();

        AppendOptions(parentId: null, prefix: string.Empty);
        return options;

        void AppendOptions(string? parentId, string prefix)
        {
            var children = childrenByParent[parentId].ToList();
            if (children.Count == 0)
            {
                return;
            }

            foreach (var child in children)
            {
                var label = string.IsNullOrWhiteSpace(prefix) ? child.Name : $"{prefix} / {child.Name}";
                options.Add(new LibraryFolderOptionViewModel(child.Id, label));
                AppendOptions(child.Id, label);
            }
        }
    }

    private static bool ContainsSelection(
        IEnumerable<LibraryFolderNodeViewModel> nodes,
        string selectedFolderId) =>
        nodes.Any(node =>
            string.Equals(node.Id, selectedFolderId, StringComparison.Ordinal)
            || ContainsSelection(node.Children, selectedFolderId));

    private static Dictionary<string, int> BuildTotalCountByFolder(
        IReadOnlyList<StoredLibraryFolder> folders,
        IReadOnlyCollection<LibraryCardViewModel> cards)
    {
        var directCountByFolder = cards
            .Where(card => !string.IsNullOrWhiteSpace(card.FolderId))
            .GroupBy(card => card.FolderId!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var childrenByParent = BuildChildrenLookup(folders);
        var totalCountByFolder = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var folder in folders)
        {
            ComputeTotalCount(folder.Id);
        }

        return totalCountByFolder;

        int ComputeTotalCount(string folderId)
        {
            if (totalCountByFolder.TryGetValue(folderId, out var existingCount))
            {
                return existingCount;
            }

            var childCount = childrenByParent[folderId].Sum(child => ComputeTotalCount(child.Id));
            var totalCount = directCountByFolder.GetValueOrDefault(folderId) + childCount;
            totalCountByFolder[folderId] = totalCount;
            return totalCount;
        }
    }

    private static ILookup<string?, StoredLibraryFolder> BuildChildrenLookup(IReadOnlyList<StoredLibraryFolder> folders) =>
        folders
            .OrderBy(folder => folder.DisplayOrder)
            .ThenBy(folder => folder.Name, StringComparer.OrdinalIgnoreCase)
            .ToLookup(folder => folder.ParentId, StringComparer.Ordinal);

    private static string? ResolveChipParentId(
        string selectedFolderId,
        IReadOnlyDictionary<string, StoredLibraryFolder> foldersById)
    {
        if (string.Equals(selectedFolderId, LibrarySelectionKeys.All, StringComparison.Ordinal))
        {
            return null;
        }

        return foldersById.TryGetValue(selectedFolderId, out var folder)
            ? folder.ParentId
            : null;
    }
}
