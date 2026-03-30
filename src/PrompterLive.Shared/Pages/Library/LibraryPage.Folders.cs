using Microsoft.Extensions.Logging;
using PrompterLive.Shared.Components.Library;

namespace PrompterLive.Shared.Pages;

public partial class LibraryPage
{
    private async Task SelectFolder(string folderId)
    {
        Logger.LogInformation(SelectFolderLogTemplate, folderId);
        _selectedFolderId = folderId;
        RebuildLibraryView();
        await PersistViewStateAsync();
    }

    private void StartCreateFolder()
    {
        var draftParentId = ResolveDraftParentId();
        Logger.LogInformation(StartCreateFolderLogTemplate, draftParentId);
        _isCreatingFolder = true;
        _folderDraftName = string.Empty;
        _folderDraftParentId = draftParentId;
    }

    private void CancelCreateFolder()
    {
        Logger.LogInformation(CancelCreateFolderLogMessage);
        _isCreatingFolder = false;
        _folderDraftName = string.Empty;
        _folderDraftParentId = ResolveDraftParentId();
    }

    private void UpdateFolderDraftName(string name)
    {
        _folderDraftName = name;
    }

    private void UpdateFolderDraftParent(string parentId)
    {
        _folderDraftParentId = string.IsNullOrWhiteSpace(parentId)
            ? LibrarySelectionKeys.Root
            : parentId;
    }

    private Task SubmitCreateFolderAsync()
    {
        var folderName = _folderDraftName.Trim();
        if (folderName.Length == 0)
        {
            return Task.CompletedTask;
        }

        return RunLibraryOperationAsync(
            CreateFolderOperation,
            CreateFolderMessage,
            async () =>
            {
                await Bootstrapper.EnsureReadyAsync();
                var parentId = NormalizeDraftParentId(_folderDraftParentId);
                var folder = await LibraryFolderRepository.CreateAsync(folderName, parentId);
                ExpandCreatedFolderPath(folder.Id, parentId);
                ResetFolderDraftState(folder.Id);
                Logger.LogInformation(FolderCreatedLogTemplate, folder.Id, parentId ?? LibrarySelectionKeys.Root);
                await LoadLibraryAsync();
                await PersistViewStateAsync();
            });
    }

    private void ExpandCreatedFolderPath(string folderId, string? parentId)
    {
        if (parentId is not null)
        {
            _expandedFolderIds.Add(parentId);
        }

        _expandedFolderIds.Add(folderId);
    }

    private void ResetFolderDraftState(string selectedFolderId)
    {
        _isCreatingFolder = false;
        _folderDraftName = string.Empty;
        _folderDraftParentId = ResolveDraftParentId();
        _selectedFolderId = selectedFolderId;
    }

    private string ResolveDraftParentId() =>
        NormalizeDraftParentId(_selectedFolderId) ?? LibrarySelectionKeys.Root;

    private static string? NormalizeDraftParentId(string? folderId) =>
        string.IsNullOrWhiteSpace(folderId) || string.Equals(folderId, LibrarySelectionKeys.All, StringComparison.Ordinal)
            ? null
            : string.Equals(folderId, LibrarySelectionKeys.Root, StringComparison.Ordinal)
                ? null
                : folderId;
}
