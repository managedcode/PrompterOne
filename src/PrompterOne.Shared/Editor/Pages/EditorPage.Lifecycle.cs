namespace PrompterOne.Shared.Pages;

public partial class EditorPage : IDisposable
{
    public void Dispose()
    {
        CancelDraftAnalysis();
        CancelAutosave();
    }
}
