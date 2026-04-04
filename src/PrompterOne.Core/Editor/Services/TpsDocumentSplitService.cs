using System.Text.RegularExpressions;
using PrompterOne.Core.Models.Editor;

namespace PrompterOne.Core.Services.Editor;

public sealed partial class TpsDocumentSplitService
{
    private const string TitleGroupName = "title";
    private readonly TpsFrontMatterDocumentService _frontMatterService = new();

    [GeneratedRegex(@"^#(?!#)\s+(?<title>[^\r\n]+?)\s*$", RegexOptions.Multiline)]
    private static partial Regex TopLevelHeadingRegex();

    [GeneratedRegex(@"^##(?!#)\s+(?<title>[^\r\n]+?)\s*$", RegexOptions.Multiline)]
    private static partial Regex SegmentHeadingRegex();

    public IReadOnlyList<TpsDocumentSplitDocument> Split(string? source, TpsDocumentSplitMode mode)
    {
        var document = _frontMatterService.Parse(source);
        if (string.IsNullOrWhiteSpace(document.Body))
        {
            return [];
        }

        var matches = ResolveHeadingRegex(mode).Matches(document.Body);
        if (matches.Count == 0)
        {
            return [];
        }

        var splitDocuments = new List<TpsDocumentSplitDocument>(matches.Count);
        for (var index = 0; index < matches.Count; index++)
        {
            var match = matches[index];
            var title = ResolveSplitTitle(match.Groups[TitleGroupName].Value);
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var sliceStart = index == 0 ? 0 : match.Index;
            var sliceEnd = index + 1 < matches.Count ? matches[index + 1].Index : document.Body.Length;
            var sliceBody = document.Body[sliceStart..sliceEnd].Trim();
            if (string.IsNullOrWhiteSpace(sliceBody))
            {
                continue;
            }

            splitDocuments.Add(new TpsDocumentSplitDocument(
                index + 1,
                title,
                _frontMatterService.Build(BuildChildMetadata(document.Metadata, title), sliceBody)));
        }

        return splitDocuments;
    }

    private static IReadOnlyDictionary<string, string> BuildChildMetadata(
        IReadOnlyDictionary<string, string> sourceMetadata,
        string title)
    {
        var metadata = new Dictionary<string, string>(sourceMetadata, StringComparer.OrdinalIgnoreCase)
        {
            [TpsFrontMatterDocumentService.MetadataKeys.Title] = title
        };

        metadata.Remove(TpsFrontMatterDocumentService.MetadataKeys.Duration);
        return metadata;
    }

    private static Regex ResolveHeadingRegex(TpsDocumentSplitMode mode) =>
        mode == TpsDocumentSplitMode.TopLevelHeading
            ? TopLevelHeadingRegex()
            : SegmentHeadingRegex();

    private static string ResolveSplitTitle(string rawHeadingTitle)
    {
        var trimmedTitle = rawHeadingTitle.Trim();
        if (!trimmedTitle.StartsWith("[", StringComparison.Ordinal) ||
            !trimmedTitle.EndsWith("]", StringComparison.Ordinal))
        {
            return trimmedTitle;
        }

        var headerParts = TpsEscaping.SplitHeaderParts(TpsEscaping.Protect(trimmedTitle[1..^1]));
        return headerParts.FirstOrDefault(static part => !string.IsNullOrWhiteSpace(part))?.Trim() ?? string.Empty;
    }
}
