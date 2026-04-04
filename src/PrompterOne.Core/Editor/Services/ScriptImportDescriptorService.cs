using PrompterOne.Core.Models.Editor;
using PrompterOne.Core.Models.Workspace;

namespace PrompterOne.Core.Services.Editor;

public sealed class ScriptImportDescriptorService
{
    private const string UnsupportedFileNameMessage = "Only .tps, .tps.md, .md.tps, .md, and .txt files are supported.";

    private static readonly string[] SupportedFileNameSuffixes =
    [
        ".tps.md",
        ".md.tps",
        ".tps",
        ".md",
        ".txt"
    ];

    private readonly TpsFrontMatterDocumentService _frontMatterService = new();

    public bool CanImport(string? fileName) =>
        ResolveSupportedSuffix(NormalizeFileName(fileName)) is not null;

    public ScriptImportDescriptor Build(string? fileName, string? text)
    {
        var normalizedFileName = NormalizeSupportedFileName(fileName);
        var normalizedText = text ?? string.Empty;
        var title = _frontMatterService.ResolveTitle(normalizedText, ResolveFallbackTitle(normalizedFileName));

        return new ScriptImportDescriptor(
            Title: title,
            Text: normalizedText,
            DocumentName: normalizedFileName);
    }

    private static string NormalizeSupportedFileName(string? fileName)
    {
        var normalizedFileName = NormalizeFileName(fileName);
        if (ResolveSupportedSuffix(normalizedFileName) is null)
        {
            throw new ArgumentException(UnsupportedFileNameMessage, nameof(fileName));
        }

        return normalizedFileName;
    }

    private static string NormalizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        return Path.GetFileName(fileName.Trim());
    }

    private static string ResolveFallbackTitle(string fileName)
    {
        var suffix = ResolveSupportedSuffix(fileName);
        if (suffix is null)
        {
            return ScriptWorkspaceState.UntitledScriptTitle;
        }

        var stem = fileName[..^suffix.Length].Trim();
        return string.IsNullOrWhiteSpace(stem)
            ? ScriptWorkspaceState.UntitledScriptTitle
            : stem;
    }

    private static string? ResolveSupportedSuffix(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return SupportedFileNameSuffixes.FirstOrDefault(
            suffix => fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }
}
