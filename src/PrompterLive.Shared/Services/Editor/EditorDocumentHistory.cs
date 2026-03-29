using PrompterLive.Core.Models.Editor;

namespace PrompterLive.Shared.Services.Editor;

public sealed class EditorDocumentHistory
{
    private const int MaxSnapshots = 100;
    private readonly List<EditorHistorySnapshot> _snapshots = [];
    private int _cursor = -1;

    public bool IsInitialized => _cursor >= 0;

    public bool CanRedo => _cursor >= 0 && _cursor < _snapshots.Count - 1;

    public bool CanUndo => _cursor > 0;

    public void Reset(string text, EditorSelectionRange selection)
    {
        _snapshots.Clear();
        _snapshots.Add(new EditorHistorySnapshot(text, selection));
        _cursor = 0;
    }

    public bool TryRedo(out EditorHistorySnapshot snapshot)
    {
        if (!CanRedo)
        {
            snapshot = EditorHistorySnapshot.Empty;
            return false;
        }

        _cursor++;
        snapshot = _snapshots[_cursor];
        return true;
    }

    public bool TryRecord(string text, EditorSelectionRange selection)
    {
        var nextSnapshot = new EditorHistorySnapshot(text, selection);
        if (_cursor < 0)
        {
            Reset(text, selection);
            return false;
        }

        if (_snapshots[_cursor] == nextSnapshot)
        {
            return false;
        }

        if (CanRedo)
        {
            _snapshots.RemoveRange(_cursor + 1, _snapshots.Count - (_cursor + 1));
        }

        _snapshots.Add(nextSnapshot);
        if (_snapshots.Count > MaxSnapshots)
        {
            _snapshots.RemoveAt(0);
        }
        else
        {
            _cursor++;
        }

        _cursor = _snapshots.Count - 1;
        return true;
    }

    public bool TryUndo(out EditorHistorySnapshot snapshot)
    {
        if (!CanUndo)
        {
            snapshot = EditorHistorySnapshot.Empty;
            return false;
        }

        _cursor--;
        snapshot = _snapshots[_cursor];
        return true;
    }

    public void UpdateSelection(EditorSelectionRange selection)
    {
        if (_cursor < 0)
        {
            return;
        }

        var current = _snapshots[_cursor];
        if (current.Selection == selection)
        {
            return;
        }

        _snapshots[_cursor] = current with { Selection = selection };
    }
}

public sealed record EditorHistorySnapshot(string Text, EditorSelectionRange Selection)
{
    public static EditorHistorySnapshot Empty { get; } = new(string.Empty, EditorSelectionRange.Empty);
}
