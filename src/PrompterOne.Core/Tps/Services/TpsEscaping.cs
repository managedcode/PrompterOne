using System.Text;

namespace PrompterOne.Core.Services;

internal static class TpsEscaping
{
    private const char EscapedAsterisk = '\uE005';
    private const char EscapedBackslash = '\uE006';
    private const char EscapedBracketClose = '\uE002';
    private const char EscapedBracketOpen = '\uE001';
    private const char EscapedPipe = '\uE003';
    private const char EscapedSlash = '\uE004';

    public static string Protect(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace(@"\\", EscapedBackslash.ToString(), StringComparison.Ordinal)
            .Replace(@"\[", EscapedBracketOpen.ToString(), StringComparison.Ordinal)
            .Replace(@"\]", EscapedBracketClose.ToString(), StringComparison.Ordinal)
            .Replace(@"\|", EscapedPipe.ToString(), StringComparison.Ordinal)
            .Replace(@"\/", EscapedSlash.ToString(), StringComparison.Ordinal)
            .Replace(@"\*", EscapedAsterisk.ToString(), StringComparison.Ordinal);
    }

    public static string Restore(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace(EscapedBracketOpen, '[')
            .Replace(EscapedBracketClose, ']')
            .Replace(EscapedPipe, '|')
            .Replace(EscapedSlash, '/')
            .Replace(EscapedAsterisk, '*')
            .Replace(EscapedBackslash, '\\');
    }

    public static IReadOnlyList<string> SplitHeaderParts(string rawHeaderContent)
    {
        var parts = new List<string>();
        var builder = new StringBuilder();

        foreach (var character in rawHeaderContent)
        {
            if (character == '|')
            {
                parts.Add(Restore(builder.ToString()).Trim());
                builder.Clear();
                continue;
            }

            builder.Append(character);
        }

        parts.Add(Restore(builder.ToString()).Trim());
        return parts;
    }
}
