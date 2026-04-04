namespace PrompterOne.Core.Services.Editor;

public static class ScriptDocumentFileTypes
{
    public const string AcceptValue = ".tps,.tps.md,.md.tps,.md,.txt";
    public const string DefaultExtension = ".tps";
    public const string SavePickerDescription = "PrompterOne script";
    public const string TextMimeType = "text/plain";

    public static IReadOnlyList<string> SupportedFileNameSuffixes { get; } =
    [
        ".tps.md",
        ".md.tps",
        ".tps",
        ".md",
        ".txt"
    ];

    public static string NormalizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        return Path.GetFileName(fileName.Trim());
    }

    public static string? ResolveSupportedSuffix(string? fileName)
    {
        var normalizedFileName = NormalizeFileName(fileName);
        if (string.IsNullOrWhiteSpace(normalizedFileName))
        {
            return null;
        }

        return SupportedFileNameSuffixes.FirstOrDefault(
            suffix => normalizedFileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }
}
