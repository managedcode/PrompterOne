using PrompterOne.Shared.Components.Library;

namespace PrompterOne.Shared.Services.Library;

public sealed record LibraryViewState(
    string SelectedFolderId,
    LibrarySortMode SortMode,
    IReadOnlyList<string> ExpandedFolderIds)
{
    public static LibraryViewState Default { get; } = new(
        SelectedFolderId: LibrarySelectionKeys.All,
        SortMode: LibrarySortMode.Name,
        ExpandedFolderIds: []);
}
