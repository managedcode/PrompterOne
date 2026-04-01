namespace PrompterOne.Core.Models.Editor;

public readonly record struct EditorSelectionRange(int Start, int End)
{
    public int OrderedStart => Math.Min(Start, End);

    public int OrderedEnd => Math.Max(Start, End);

    public bool HasSelection => OrderedEnd > OrderedStart;

    public static EditorSelectionRange Empty => new(0, 0);
}
