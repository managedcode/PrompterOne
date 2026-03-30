using System.Globalization;
using System.Text;
using PrompterLive.Core.Models.Tps;

namespace PrompterLive.Core.Services;

public class TpsExporter
{
    private const char HeaderDelimiterCharacter = '-';
    private const int HeaderDelimiterLength = 3;

    public Task<string> ExportAsync(TpsDocument document)
    {
        var sb = new StringBuilder();

        // Export metadata as YAML front matter
        if (document.Metadata != null && document.Metadata.Any())
        {
            sb.AppendLine(CreateHeaderDelimiter());
            foreach (var kvp in document.Metadata)
            {
                // Quote string values that contain spaces or special characters
                var value = kvp.Value;
                if (value.Contains(' ') || value.Contains(':') || value.Contains('-'))
                {
                    value = FormattableString.Invariant($"\"{value}\"");
                }

                sb.Append(CultureInfo.InvariantCulture, $"{kvp.Key}: {value}");
                sb.AppendLine();
            }
            sb.AppendLine(CreateHeaderDelimiter());
            sb.AppendLine();
        }

        // Export segments
        foreach (var segment in document.Segments)
        {
            // Build segment header
            var segmentHeader = new StringBuilder();
            segmentHeader.Append(CultureInfo.InvariantCulture, $"## [{segment.Name}");

            if (segment.TargetWPM.HasValue)
            {
                segmentHeader.Append(CultureInfo.InvariantCulture, $"|{segment.TargetWPM}WPM");
            }

            if (!string.IsNullOrEmpty(segment.Emotion))
            {
                segmentHeader.Append(CultureInfo.InvariantCulture, $"|{segment.Emotion}");
            }

            if (segment.Duration.HasValue)
            {
                var duration = segment.Duration.Value;
                segmentHeader.Append(CultureInfo.InvariantCulture, $"|{duration.Minutes:D2}:{duration.Seconds:D2}");
            }

            segmentHeader.Append(']');
            sb.AppendLine(segmentHeader.ToString());
            sb.AppendLine();

            // Export blocks
            if (segment.Blocks != null && segment.Blocks.Any())
            {
                foreach (var block in segment.Blocks)
                {
                    // Build block header
                    var blockHeader = new StringBuilder();
                    blockHeader.Append(CultureInfo.InvariantCulture, $"### [{block.Name}");

                    if (block.TargetWPM.HasValue)
                    {
                        blockHeader.Append(CultureInfo.InvariantCulture, $"|{block.TargetWPM}WPM");
                    }

                    if (!string.IsNullOrEmpty(block.Emotion))
                    {
                        blockHeader.Append(CultureInfo.InvariantCulture, $"|{block.Emotion}");
                    }

                    blockHeader.Append(']');
                    sb.AppendLine(blockHeader.ToString());
                    sb.AppendLine();

                    // Export block content or phrases
                    if (block.Phrases != null && block.Phrases.Any())
                    {
                        foreach (var phrase in block.Phrases)
                        {
                            if (!string.IsNullOrEmpty(phrase.Content))
                            {
                                sb.AppendLine(phrase.Content);
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(block.Content))
                    {
                        sb.AppendLine(block.Content);
                    }

                    sb.AppendLine();
                }
            }
            else if (!string.IsNullOrEmpty(segment.Content))
            {
                // Export segment content directly if no blocks
                sb.AppendLine(segment.Content);
                sb.AppendLine();
            }
        }

        return Task.FromResult(sb.ToString().TrimEnd());
    }

    public async Task ExportToFileAsync(TpsDocument document, string filePath)
    {
        var content = await ExportAsync(document);
        await File.WriteAllTextAsync(filePath, content);
    }

    private static string CreateHeaderDelimiter() => new(HeaderDelimiterCharacter, HeaderDelimiterLength);
}
