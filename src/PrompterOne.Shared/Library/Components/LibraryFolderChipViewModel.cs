using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Components.Library;

public sealed record LibraryFolderChipViewModel(
    string Id,
    string Name,
    int TotalCount,
    bool IsSelected)
{
    public string TestId => UiTestIds.Library.FolderChip(Id);
}
