using PrompterLive.Shared.Components.Library;

namespace PrompterLive.Shared.Pages;

public partial class LibraryPage
{
    private Task CreateScriptAsync() =>
        RunLibraryOperationAsync(
            CreateScriptOperation,
            CreateScriptMessage,
            async () =>
            {
                await Bootstrapper.EnsureReadyAsync();
                await SessionService.NewAsync();
                Navigation.NavigateTo("/editor");
            });

    private Task OpenScriptAsync(string id) =>
        RunLibraryOperationAsync(
            OpenScriptOperation,
            OpenScriptMessage,
            async () =>
            {
                await Bootstrapper.EnsureReadyAsync();
                var document = await ScriptRepository.GetAsync(id);
                if (document is null)
                {
                    return;
                }

                await SessionService.OpenAsync(document);
                Navigation.NavigateTo($"/editor?id={Uri.EscapeDataString(id)}");
            });

    private Task LearnScriptAsync(string id)
    {
        Navigation.NavigateTo($"/learn?id={Uri.EscapeDataString(id)}");
        return Task.CompletedTask;
    }

    private Task ReadScriptAsync(string id)
    {
        Navigation.NavigateTo($"/teleprompter?id={Uri.EscapeDataString(id)}");
        return Task.CompletedTask;
    }

    private Task DuplicateScriptAsync(string id) =>
        RunLibraryOperationAsync(
            DuplicateScriptOperation,
            DuplicateScriptMessage,
            async () =>
            {
                await Bootstrapper.EnsureReadyAsync();
                var document = await ScriptRepository.GetAsync(id);
                if (document is null)
                {
                    return;
                }

                await ScriptRepository.SaveAsync(
                    title: $"{document.Title} Copy",
                    text: document.Text,
                    documentName: null,
                    existingId: null,
                    folderId: document.FolderId);
                await LoadLibraryAsync();
            });

    private Task MoveScriptAsync(LibraryMoveRequest request) =>
        RunLibraryOperationAsync(
            MoveScriptOperation,
            MoveScriptMessage,
            async () =>
            {
                await Bootstrapper.EnsureReadyAsync();
                await ScriptRepository.MoveToFolderAsync(request.ScriptId, request.FolderId);
                await LoadLibraryAsync();
            });

    private Task DeleteScriptAsync(string id) =>
        RunLibraryOperationAsync(
            DeleteScriptOperation,
            DeleteScriptMessage,
            async () =>
            {
                await Bootstrapper.EnsureReadyAsync();
                await ScriptRepository.DeleteAsync(id);

                if (string.Equals(SessionService.State.ScriptId, id, StringComparison.Ordinal))
                {
                    await SessionService.NewAsync();
                }

                await LoadLibraryAsync();
            });
}
