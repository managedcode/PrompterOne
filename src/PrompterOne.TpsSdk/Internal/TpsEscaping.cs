namespace ManagedCode.Tps.Internal;

internal static class TpsEscaping
{
    private const char BracketClosePlaceholder = '\uE002';
    private const char BracketOpenPlaceholder = '\uE001';
    private const char PipePlaceholder = '\uE003';
    private const char SlashPlaceholder = '\uE004';
    private const char AsteriskPlaceholder = '\uE005';
    private const char BackslashPlaceholder = '\uE006';

    public static string Protect(string text) => text
        .Replace(@"\\", BackslashPlaceholder.ToString(), StringComparison.Ordinal)
        .Replace(@"\[", BracketOpenPlaceholder.ToString(), StringComparison.Ordinal)
        .Replace(@"\]", BracketClosePlaceholder.ToString(), StringComparison.Ordinal)
        .Replace(@"\|", PipePlaceholder.ToString(), StringComparison.Ordinal)
        .Replace(@"\/", SlashPlaceholder.ToString(), StringComparison.Ordinal)
        .Replace(@"\*", AsteriskPlaceholder.ToString(), StringComparison.Ordinal);

    public static string Restore(string text) => text
        .Replace(BracketOpenPlaceholder.ToString(), "[", StringComparison.Ordinal)
        .Replace(BracketClosePlaceholder.ToString(), "]", StringComparison.Ordinal)
        .Replace(PipePlaceholder.ToString(), "|", StringComparison.Ordinal)
        .Replace(SlashPlaceholder.ToString(), "/", StringComparison.Ordinal)
        .Replace(AsteriskPlaceholder.ToString(), "*", StringComparison.Ordinal)
        .Replace(BackslashPlaceholder.ToString(), @"\", StringComparison.Ordinal);

    public static List<HeaderPart> SplitHeaderPartsDetailed(string rawHeaderContent)
    {
        var parts = new List<HeaderPart>();
        var current = new System.Text.StringBuilder();
        var partStart = 0;

        for (var index = 0; index < rawHeaderContent.Length; index++)
        {
            var character = rawHeaderContent[index];
            if (character == '\\' && index + 1 < rawHeaderContent.Length)
            {
                current.Append(rawHeaderContent[index + 1]);
                index++;
                continue;
            }

            if (character == '|')
            {
                parts.Add(new HeaderPart(current.ToString().Trim(), partStart, index));
                current.Clear();
                partStart = index + 1;
                continue;
            }

            current.Append(character);
        }

        parts.Add(new HeaderPart(current.ToString().Trim(), partStart, rawHeaderContent.Length));
        return parts;
    }
}

internal sealed record HeaderPart(string Value, int Start, int End);
