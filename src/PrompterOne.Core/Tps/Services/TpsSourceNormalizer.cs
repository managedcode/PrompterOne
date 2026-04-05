namespace PrompterOne.Core.Services;

internal static class TpsSourceNormalizer
{
    public static string NormalizeLineEndings(string? value) =>
        value?.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n') ?? string.Empty;
}
