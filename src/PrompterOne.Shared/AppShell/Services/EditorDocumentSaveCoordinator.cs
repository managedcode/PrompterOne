namespace PrompterOne.Shared.Services;

public sealed class EditorDocumentSaveCoordinator
{
    private Func<CancellationToken, Task>? _saveHandler;

    public void Register(Func<CancellationToken, Task> saveHandler)
    {
        ArgumentNullException.ThrowIfNull(saveHandler);
        _saveHandler = saveHandler;
    }

    public void Unregister(Func<CancellationToken, Task> saveHandler)
    {
        ArgumentNullException.ThrowIfNull(saveHandler);

        if (_saveHandler == saveHandler)
        {
            _saveHandler = null;
        }
    }

    public Task RequestSaveAsync(CancellationToken cancellationToken = default) =>
        _saveHandler is null
            ? Task.CompletedTask
            : _saveHandler(cancellationToken);
}
