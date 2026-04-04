namespace PrompterOne.Core.Models.Editor;

public sealed record EditorDroppedFilesRequest(
    IReadOnlyList<EditorDroppedFile> Files,
    IReadOnlyList<string> RejectedFileNames)
{
    public static EditorDroppedFilesRequest Empty { get; } = new(Array.Empty<EditorDroppedFile>(), Array.Empty<string>());
}
