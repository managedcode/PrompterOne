using System.Text.RegularExpressions;
using ManagedCode.Tps;

namespace PrompterOne.Web.Tests;

public sealed class EditorTpsLanguageSpecContractTests
{
    private static readonly Regex ArrayRegexTemplate = new(
        @"const\s+(?<name>[A-Za-z0-9_]+)\s*=\s*\[(?<values>.*?)\];",
        RegexOptions.Singleline | RegexOptions.Compiled);

    [Theory]
    [InlineData("archetypeNames", nameof(TpsSpec.Archetypes))]
    [InlineData("emotionTagNames", nameof(TpsSpec.Emotions))]
    [InlineData("volumeTagNames", nameof(TpsSpec.VolumeLevels))]
    [InlineData("deliveryTagNames", nameof(TpsSpec.DeliveryModes))]
    [InlineData("articulationTagNames", nameof(TpsSpec.ArticulationStyles))]
    [InlineData("speedTagNames", nameof(TpsSpec.RelativeSpeedTags))]
    public void MonacoTpsLanguageSpec_ContainsTheSameVocabularyAsVendoredSdk(string arrayName, string sdkPropertyName)
    {
        var source = File.ReadAllText(GetLanguageSpecPath());
        var actual = ExtractStringArray(source, arrayName);
        var expected = GetSdkVocabulary(sdkPropertyName);

        Assert.Equal(expected, actual, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetLanguageSpecPath() =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/PrompterOne.Shared/wwwroot/editor/editor-monaco-tps-language-spec.js"));

    private static IReadOnlyList<string> ExtractStringArray(string source, string arrayName)
    {
        var match = ArrayRegexTemplate.Matches(source)
            .Cast<Match>()
            .Single(candidate => string.Equals(candidate.Groups["name"].Value, arrayName, StringComparison.Ordinal));

        return Regex.Matches(match.Groups["values"].Value, "\"([^\"]+)\"")
            .Select(token => token.Groups[1].Value)
            .ToArray();
    }

    private static IReadOnlyList<string> GetSdkVocabulary(string sdkPropertyName)
    {
        return sdkPropertyName switch
        {
            nameof(TpsSpec.Archetypes) => TpsSpec.Archetypes.ToArray(),
            nameof(TpsSpec.Emotions) => TpsSpec.Emotions.ToArray(),
            nameof(TpsSpec.VolumeLevels) => TpsSpec.VolumeLevels.ToArray(),
            nameof(TpsSpec.DeliveryModes) => TpsSpec.DeliveryModes.ToArray(),
            nameof(TpsSpec.ArticulationStyles) => TpsSpec.ArticulationStyles.ToArray(),
            nameof(TpsSpec.RelativeSpeedTags) => TpsSpec.RelativeSpeedTags.ToArray(),
            _ => throw new InvalidOperationException($"Unknown TPS SDK vocabulary property '{sdkPropertyName}'.")
        };
    }
}
