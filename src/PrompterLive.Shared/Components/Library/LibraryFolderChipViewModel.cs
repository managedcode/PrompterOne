using PrompterLive.Shared.Contracts;

namespace PrompterLive.Shared.Components.Library;

public sealed record LibraryFolderChipViewModel(
    string Id,
    string Name,
    int TotalCount,
    bool IsSelected)
{
    public string TestId => UiTestIds.Library.FolderChip(Id);
}
