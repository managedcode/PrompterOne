namespace PrompterLive.Shared.Services;

internal static class LegacyLibrarySeedCatalog
{
    public const string CleanupVersion = "2026-03-31-runtime-cleanup-v1";

    private static readonly HashSet<string> LegacyDocumentIds = new(StringComparer.Ordinal)
    {
        "rsvp-tech-demo",
        "ted-leadership",
        "security-incident",
        "green-architecture",
        "quantum-computing",
        "comprehensive-demo"
    };

    private static readonly HashSet<string> LegacyFolderIds = new(StringComparer.Ordinal)
    {
        "presentations",
        "product",
        "investors",
        "podcasts",
        "news",
        "ted",
        "internal"
    };

    public static bool IsLegacyDocument(BrowserStoredScriptDocumentDto document)
    {
        return LegacyDocumentIds.Contains(document.Id ?? string.Empty);
    }

    public static bool IsLegacyFolder(BrowserStoredLibraryFolderDto folder)
    {
        return LegacyFolderIds.Contains(folder.Id ?? string.Empty);
    }
}
