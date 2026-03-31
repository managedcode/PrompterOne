using PrompterLive.Core.Models.Documents;

namespace PrompterLive.Core.Tests;

internal static class CoreTestSeedData
{
    public static class Scripts
    {
        public const string DemoId = "test-product-launch-script";
        public const string SecurityIncidentId = "test-security-incident-script";
    }

    public static IReadOnlyList<StoredScriptDocument> CreateDocuments() =>
    [
        CreateDocument(
            Scripts.DemoId,
            "Product Launch",
            "test-product-launch.tps",
            new DateTimeOffset(2026, 3, 25, 9, 0, 0, TimeSpan.Zero)),
        CreateDocument(
            Scripts.SecurityIncidentId,
            "Security Incident",
            "test-security-incident.tps",
            new DateTimeOffset(2026, 3, 24, 8, 30, 0, TimeSpan.Zero))
    ];

    private static StoredScriptDocument CreateDocument(
        string id,
        string title,
        string documentName,
        DateTimeOffset updatedAt)
    {
        return new StoredScriptDocument(
            Id: id,
            Title: title,
            Text: File.ReadAllText(GetScriptPath(documentName)),
            DocumentName: documentName,
            UpdatedAt: updatedAt,
            FolderId: null);
    }

    private static string GetScriptPath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../tests/TestData/Scripts",
            fileName));
    }
}
