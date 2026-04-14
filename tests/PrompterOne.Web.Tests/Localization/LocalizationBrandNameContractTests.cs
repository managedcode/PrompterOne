using System.Xml.Linq;

namespace PrompterOne.Web.Tests;

public sealed class LocalizationBrandNameContractTests
{
    private const string BrandToken = "PrompterOne";
    private const string DefaultResourceFileName = "SharedResource.resx";
    private static readonly string RepoRoot = ResolveRepoRoot();
    private static readonly string LocalizationDirectory = Path.Combine(RepoRoot, "src", "PrompterOne.Shared", "Localization");
    private static readonly string[] ResourcePaths = Directory.EnumerateFiles(LocalizationDirectory, "SharedResource*.resx", SearchOption.TopDirectoryOnly)
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();

    private static readonly string[] ForbiddenBrandVariants =
    [
        "Prompter One",
        "SuflerOne",
        "СуфлерOne"
    ];

    private static readonly string[] RequiredEditorLocalizationKeys =
    [
        "ImportScriptMessage",
        "ImportScriptUnsupportedDetail",
        "EditorDropUnsupportedDetail",
        "EditorLoadMessage",
        "EditorPersistDraftMessage",
        "EditorSaveFileMessage",
        "EditorSplitDraftMessage",
        "EditorSplitNoMatchesMessage",
        "EditorSyntaxMessage"
    ];

    private static readonly IReadOnlyDictionary<string, string> DefaultResourceValues = LoadResourceValues(
        Path.Combine(LocalizationDirectory, DefaultResourceFileName));

    private static readonly string[] BrandBearingKeys = DefaultResourceValues
        .Where(entry => entry.Value.Contains(BrandToken, StringComparison.Ordinal))
        .Select(entry => entry.Key)
        .OrderBy(key => key, StringComparer.Ordinal)
        .ToArray();

    public static IEnumerable<string> LocalizedResourcePaths =>
        ResourcePaths
            .Where(resourcePath => !string.Equals(Path.GetFileName(resourcePath), DefaultResourceFileName, StringComparison.Ordinal));

    public static IEnumerable<(string ResourcePath, string ForbiddenVariant)> ForbiddenVariantCases =>
        ResourcePaths.SelectMany(
            resourcePath => ForbiddenBrandVariants.Select(forbiddenVariant => (resourcePath, forbiddenVariant)));

    public static IEnumerable<(string ResourcePath, string ResourceKey)> BrandBearingResourceCases =>
        ResourcePaths.SelectMany(
            resourcePath =>
            {
                var resourceValues = LoadResourceValues(resourcePath);
                return BrandBearingKeys
                    .Where(resourceValues.ContainsKey)
                    .Select(resourceKey => (resourcePath, resourceKey));
            });

    public static IEnumerable<(string ResourcePath, string ResourceKey)> RequiredEditorLocalizationCases =>
        ResourcePaths.SelectMany(
            resourcePath => RequiredEditorLocalizationKeys.Select(resourceKey => (resourcePath, resourceKey)));

    [Test]
    [MethodDataSource(nameof(ForbiddenVariantCases))]
    public void SharedResources_DoNotContainLocalizedBrandVariants(string resourcePath, string forbiddenVariant)
    {
        var resourceFileText = File.ReadAllText(resourcePath);

        Assert.DoesNotContain(forbiddenVariant, resourceFileText, StringComparison.Ordinal);
    }

    [Test]
    [MethodDataSource(nameof(BrandBearingResourceCases))]
    public void SharedResources_KeepExactBrandToken_ForBrandBearingValues(string resourcePath, string resourceKey)
    {
        var resourceValues = LoadResourceValues(resourcePath);
        var resourceValue = resourceValues[resourceKey];

        Assert.Contains(
            BrandToken,
            resourceValue,
            StringComparison.Ordinal);
    }

    [Test]
    [MethodDataSource(nameof(RequiredEditorLocalizationCases))]
    public void SharedResources_ContainEditorDiagnosticsCopy_ForEveryLocale(string resourcePath, string resourceKey)
    {
        var resourceValues = LoadResourceValues(resourcePath);

        Assert.True(resourceValues.TryGetValue(resourceKey, out var resourceValue));
        Assert.False(string.IsNullOrWhiteSpace(resourceValue));
    }

    [Test]
    [MethodDataSource(nameof(LocalizedResourcePaths))]
    public void SharedResources_ContainDefaultKeys_ForEveryLocale(string resourcePath)
    {
        var resourceValues = LoadResourceValues(resourcePath);

        var missingKeys = DefaultResourceValues.Keys
            .Where(resourceKey => !resourceValues.TryGetValue(resourceKey, out var resourceValue) || string.IsNullOrWhiteSpace(resourceValue))
            .OrderBy(resourceKey => resourceKey, StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(missingKeys);
    }

    private static IReadOnlyDictionary<string, string> LoadResourceValues(string resourcePath)
    {
        var document = XDocument.Load(resourcePath);

        return document.Root?
            .Elements("data")
            .Select(
                element => new
                {
                    Name = element.Attribute("name")?.Value,
                    Value = element.Element("value")?.Value
                })
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name) && entry.Value is not null)
            .ToDictionary(entry => entry.Name!, entry => entry.Value!, StringComparer.Ordinal)
            ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static string ResolveRepoRoot() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
}
