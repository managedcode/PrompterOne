namespace PrompterOne.Shared.Pages;

public partial class EditorPage : IDisposable
{
    protected override void OnInitialized()
    {
        EditorDocumentSaveCoordinator.Register(HandleSaveFileRequestedAsync);
    }

    public void Dispose()
    {
        EditorDocumentSaveCoordinator.Unregister(HandleSaveFileRequestedAsync);
        CancelDraftAnalysis();
        CancelAutosave();
    }
}
