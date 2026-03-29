using System.Text;
using System.Text.RegularExpressions;
using PrompterLive.Core.Models.Editor;

namespace PrompterLive.Core.Services.Editor;

public sealed class TpsFrontMatterDocumentService
{
    private static readonly Regex FrontMatterRegex = new(
        @"\A---\s*\r?\n(?<front>.*?)\r?\n---\s*\r?\n?",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly string[] PreferredMetadataOrder =
    [
        MetadataKeys.Title,
        MetadataKeys.Author,
        MetadataKeys.Profile,
        MetadataKeys.BaseWpm,
        MetadataKeys.Version,
        MetadataKeys.Created
    ];

    public TpsFrontMatterDocument Parse(string? text)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text))
        {
            return new TpsFrontMatterDocument(metadata, string.Empty, 0);
        }

        var match = FrontMatterRegex.Match(text);
        if (!match.Success)
        {
            return new TpsFrontMatterDocument(metadata, text, 0);
        }

        foreach (var line in match.Groups["front"].Value.Split(
                     '\n',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"');
            if (!string.IsNullOrWhiteSpace(key))
            {
                metadata[key] = value;
            }
        }

        return new TpsFrontMatterDocument(metadata, text[match.Length..], match.Length);
    }

    public string Build(IReadOnlyDictionary<string, string> metadata, string? body)
    {
        var builder = new StringBuilder();
        builder.AppendLine(MetadataTokens.Delimiter);

        foreach (var key in PreferredMetadataOrder.Where(metadata.ContainsKey))
        {
            builder.Append(key);
            builder.Append(MetadataTokens.Separator);
            builder.AppendLine(FormatMetadataValue(key, metadata[key]));
        }

        foreach (var entry in metadata
                     .Where(entry => !PreferredMetadataOrder.Contains(entry.Key, StringComparer.OrdinalIgnoreCase))
                     .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(entry.Key);
            builder.Append(MetadataTokens.Separator);
            builder.AppendLine(FormatMetadataValue(entry.Key, entry.Value));
        }

        builder.AppendLine(MetadataTokens.Delimiter);
        builder.AppendLine();
        builder.Append((body ?? string.Empty).TrimStart());
        return builder.ToString();
    }

    public string Upsert(string? text, IReadOnlyDictionary<string, string?> updates)
    {
        var document = Parse(text);
        var nextMetadata = new Dictionary<string, string>(document.Metadata, StringComparer.OrdinalIgnoreCase);

        foreach (var update in updates)
        {
            if (string.IsNullOrWhiteSpace(update.Key))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(update.Value))
            {
                nextMetadata.Remove(update.Key);
                continue;
            }

            nextMetadata[update.Key] = update.Value.Trim();
        }

        return Build(nextMetadata, document.Body);
    }

    public string ResolveTitle(string? text, string fallback)
    {
        var document = Parse(text);
        return document.Metadata.TryGetValue(MetadataKeys.Title, out var title) &&
               !string.IsNullOrWhiteSpace(title)
            ? title.Trim()
            : fallback;
    }

    public string ResolveValue(string? text, string key, string fallback)
    {
        var document = Parse(text);
        return document.Metadata.TryGetValue(key, out var value) &&
               !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : fallback;
    }

    private static string FormatMetadataValue(string key, string? value)
    {
        value ??= string.Empty;

        if (string.Equals(key, MetadataKeys.BaseWpm, StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(value, out _))
        {
            return value;
        }

        return $"{MetadataTokens.Quote}{value.Replace(MetadataTokens.Quote, MetadataTokens.EscapedQuote, StringComparison.Ordinal)}{MetadataTokens.Quote}";
    }

    public static class MetadataKeys
    {
        public const string Author = "author";
        public const string BaseWpm = "base_wpm";
        public const string Created = "created";
        public const string Profile = "profile";
        public const string Title = "title";
        public const string Version = "version";
    }

    private static class MetadataTokens
    {
        public const string Delimiter = "---";
        public const string EscapedQuote = "\\\"";
        public const string Quote = "\"";
        public const string Separator = ": ";
    }
}
