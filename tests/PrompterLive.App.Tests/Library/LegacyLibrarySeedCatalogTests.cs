using PrompterLive.Shared.Services;

namespace PrompterLive.App.Tests;

public sealed class LegacyLibrarySeedCatalogTests
{
    [Fact]
    public void IsLegacyDocument_DoesNotTreatTestScopedSeedFilesAsRuntimeLegacyContent()
    {
        var document = new BrowserStoredScriptDocumentDto
        {
            Id = "test-security-incident-script",
            DocumentName = "test-security-incident.tps",
            Title = "Security Incident"
        };

        Assert.False(LegacyLibrarySeedCatalog.IsLegacyDocument(document));
    }

    [Fact]
    public void IsLegacyDocument_TreatsRemovedRuntimeSeedIdsAsLegacyContent()
    {
        var document = new BrowserStoredScriptDocumentDto
        {
            Id = "security-incident",
            DocumentName = "security-incident.tps",
            Title = "Security Incident"
        };

        Assert.True(LegacyLibrarySeedCatalog.IsLegacyDocument(document));
    }
}
