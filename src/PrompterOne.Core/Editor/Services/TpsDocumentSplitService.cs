using System.Text.RegularExpressions;
using PrompterOne.Core.Models.Editor;
using PrompterOne.Core.Models.Tps;

namespace PrompterOne.Core.Services.Editor;

public sealed partial class TpsDocumentSplitService
{
    private const string TitleGroupName = "title";
    private readonly TpsDocumentReader _documentReader = new();
    private readonly TpsExporter _exporter = new();
    private readonly TpsFrontMatterDocumentService _frontMatterService = new();

    [GeneratedRegex(@"^#(?!#)\s+(?<title>[^\r\n]+?)\s*$", RegexOptions.Multiline)]
    private static partial Regex TopLevelHeadingRegex();

    [GeneratedRegex(@"^##(?!#)\s+(?<title>[^\r\n]+?)\s*$", RegexOptions.Multiline)]
    private static partial Regex SegmentHeadingRegex();

    public IReadOnlyList<TpsDocumentSplitDocument> Split(string? source, TpsDocumentSplitMode mode)
    {
        return mode == TpsDocumentSplitMode.Speaker
            ? SplitBySpeaker(source)
            : SplitByHeading(source, mode);
    }

    private IReadOnlyList<TpsDocumentSplitDocument> SplitByHeading(string? source, TpsDocumentSplitMode mode)
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

    private IReadOnlyList<TpsDocumentSplitDocument> SplitBySpeaker(string? source)
    {
        var document = _documentReader.Read(source ?? string.Empty);
        if (document.Segments.Count == 0)
        {
            return [];
        }

        var speakers = DiscoverSpeakers(document);
        var splitDocuments = new List<TpsDocumentSplitDocument>(speakers.Count);
        foreach (var speaker in speakers)
        {
            var splitDocument = CreateSpeakerSplitDocument(document, speaker, splitDocuments.Count + 1);
            if (splitDocument is not null)
            {
                splitDocuments.Add(splitDocument);
            }
        }

        return splitDocuments;
    }

    private TpsDocumentSplitDocument? CreateSpeakerSplitDocument(TpsDocument sourceDocument, string speaker, int sequence)
    {
        var segments = CreateSpeakerSegments(sourceDocument.Segments, speaker);
        if (segments.Count == 0)
        {
            return null;
        }

        var title = speaker.Trim();
        var childDocument = new TpsDocument
        {
            Metadata = new Dictionary<string, string>(BuildChildMetadata(sourceDocument.Metadata, title), StringComparer.OrdinalIgnoreCase),
            Segments = segments
        };

        var text = _exporter.ExportAsync(childDocument).GetAwaiter().GetResult();
        return new TpsDocumentSplitDocument(sequence, title, text);
    }

    private static List<TpsSegment> CreateSpeakerSegments(IReadOnlyList<TpsSegment> segments, string speaker)
    {
        var filteredSegments = new List<TpsSegment>(segments.Count);
        foreach (var segment in segments)
        {
            var filteredSegment = FilterSegment(segment, speaker);
            if (filteredSegment is not null)
            {
                filteredSegments.Add(filteredSegment);
            }
        }

        return filteredSegments;
    }

    private static TpsSegment? FilterSegment(TpsSegment segment, string speaker)
    {
        var segmentMatches = SpeakerMatches(segment.Speaker, speaker);
        var filteredBlocks = FilterBlocks(segment, speaker);
        var hasLeadingContent = segmentMatches && !string.IsNullOrWhiteSpace(segment.LeadingContent);
        var hasDirectContent = segmentMatches && filteredBlocks.Count == 0 && !string.IsNullOrWhiteSpace(segment.Content);
        if (!hasLeadingContent && !hasDirectContent && filteredBlocks.Count == 0)
        {
            return null;
        }

        return new TpsSegment
        {
            Id = segment.Id,
            Name = segment.Name,
            Content = hasDirectContent ? segment.Content : string.Empty,
            TargetWPM = segment.TargetWPM,
            Emotion = segment.Emotion,
            Speaker = segmentMatches ? segment.Speaker?.Trim() : null,
            Archetype = segment.Archetype,
            Timing = segment.Timing,
            BackgroundColor = segment.BackgroundColor,
            TextColor = segment.TextColor,
            AccentColor = segment.AccentColor,
            Duration = segment.Duration,
            LeadingContent = filteredBlocks.Count > 0 && hasLeadingContent ? segment.LeadingContent : null,
            Blocks = filteredBlocks
        };
    }

    private static List<TpsBlock> FilterBlocks(TpsSegment segment, string speaker)
    {
        var filteredBlocks = new List<TpsBlock>(segment.Blocks.Count);
        foreach (var block in segment.Blocks)
        {
            if (!SpeakerMatches(ResolveSpeaker(segment, block), speaker))
            {
                continue;
            }

            filteredBlocks.Add(new TpsBlock
            {
                Id = block.Id,
                Name = block.Name,
                Content = block.Content,
                TargetWPM = block.TargetWPM,
                Emotion = block.Emotion,
                Speaker = block.Speaker,
                Archetype = block.Archetype,
                Phrases = block.Phrases
            });
        }

        return filteredBlocks;
    }

    private static List<string> DiscoverSpeakers(TpsDocument document)
    {
        var speakers = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in document.Segments)
        {
            AddSpeaker(speakers, seen, segment.Speaker);
            foreach (var block in segment.Blocks)
            {
                AddSpeaker(speakers, seen, ResolveSpeaker(segment, block));
            }
        }

        return speakers;
    }

    private static void AddSpeaker(ICollection<string> speakers, ISet<string> seen, string? speaker)
    {
        var normalizedSpeaker = speaker?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedSpeaker) || !seen.Add(normalizedSpeaker))
        {
            return;
        }

        speakers.Add(normalizedSpeaker);
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

    private static string? ResolveSpeaker(TpsSegment segment, TpsBlock block) =>
        string.IsNullOrWhiteSpace(block.Speaker)
            ? segment.Speaker
            : block.Speaker;

    private static bool SpeakerMatches(string? candidate, string speaker) =>
        !string.IsNullOrWhiteSpace(candidate) &&
        string.Equals(candidate.Trim(), speaker.Trim(), StringComparison.OrdinalIgnoreCase);

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
