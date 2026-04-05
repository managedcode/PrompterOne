namespace ManagedCode.Tps.Internal;

internal static class TpsTextRules
{
    private static readonly HashSet<char> DashCharacters = ['-', '—', '–'];
    private static readonly HashSet<char> StandalonePunctuation = [',', '.', ';', ':', '!', '?', '-', '—', '–', '…'];

    public static bool IsStandalonePunctuationToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return token.Trim().All(StandalonePunctuation.Contains);
    }

    public static string BuildStandalonePunctuationSuffix(string token)
    {
        var trimmed = token.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return trimmed.All(DashCharacters.Contains) ? $" {trimmed}" : trimmed;
    }
}
