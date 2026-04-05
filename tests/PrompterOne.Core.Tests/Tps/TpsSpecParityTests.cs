using System.Reflection;
using ManagedCode.Tps;
using PrompterOne.Core.Services;

namespace PrompterOne.Core.Tests;

public sealed class TpsSpecParityTests
{
    private static readonly Type LocalTpsSpecType = typeof(ScriptCompiler).Assembly.GetType("PrompterOne.Core.Services.TpsSpec", throwOnError: true)!;

    [Fact]
    public void LocalTpsSpec_CatalogsStayAlignedWithVendoredSdk()
    {
        Assert.Equal(TpsSpec.DefaultBaseWpm, ReadConst<int>(nameof(TpsSpec.DefaultBaseWpm)));
        Assert.Equal(TpsSpec.MinimumWpm, ReadConst<int>(nameof(TpsSpec.MinimumWpm)));
        Assert.Equal(TpsSpec.MaximumWpm, ReadConst<int>(nameof(TpsSpec.MaximumWpm)));
        Assert.Equal(TpsSpec.ShortPauseDurationMs, ReadConst<int>(nameof(TpsSpec.ShortPauseDurationMs)));
        Assert.Equal(TpsSpec.MediumPauseDurationMs, ReadConst<int>(nameof(TpsSpec.MediumPauseDurationMs)));
        Assert.Equal(TpsSpec.DefaultEmotion, ReadConst<string>(nameof(TpsSpec.DefaultEmotion)));

        AssertVocabularyEqual(TpsSpec.Emotions, ReadStringSet(nameof(TpsSpec.Emotions)));
        AssertVocabularyEqual(TpsSpec.DeliveryModes, ReadStringSet(nameof(TpsSpec.DeliveryModes)));
        AssertVocabularyEqual(TpsSpec.VolumeLevels, ReadStringSet(nameof(TpsSpec.VolumeLevels)));
        AssertVocabularyEqual(TpsSpec.RelativeSpeedTags, ReadStringSet(nameof(TpsSpec.RelativeSpeedTags)));
        AssertVocabularyEqual(TpsSpec.ArticulationStyles, ReadStringSet(nameof(TpsSpec.ArticulationStyles)));
        AssertVocabularyEqual(TpsSpec.Archetypes, ReadStringSet(nameof(TpsSpec.Archetypes)));

        AssertDictionariesEqual(TpsSpec.DefaultSpeedOffsets, ReadDictionary<int>(nameof(TpsSpec.DefaultSpeedOffsets)));
        AssertDictionariesEqual(TpsSpec.ArchetypeRecommendedWpm, ReadDictionary<int>(nameof(TpsSpec.ArchetypeRecommendedWpm)));
        AssertDictionariesEqual(TpsSpec.EmotionHeadCues, ReadDictionary<string>(nameof(TpsSpec.EmotionHeadCues)));

        var sdkPalettes = TpsSpec.EmotionPalettes
            .ToDictionary(
                pair => pair.Key,
                pair => (pair.Value.Accent, pair.Value.Text, pair.Value.Background),
                StringComparer.OrdinalIgnoreCase);
        var localPalettes = ReadEmotionPalettes();

        AssertDictionariesEqual(sdkPalettes, localPalettes);
    }

    private static T ReadConst<T>(string name) where T : notnull
    {
        var field = LocalTpsSpecType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(field);
        return (T)field!.GetRawConstantValue()!;
    }

    private static IReadOnlyCollection<string> ReadStringSet(string propertyName)
    {
        var property = LocalTpsSpecType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(property);
        return Assert.IsAssignableFrom<IEnumerable<string>>(property!.GetValue(null)).ToArray();
    }

    private static IReadOnlyDictionary<string, TValue> ReadDictionary<TValue>(string propertyName)
    {
        var property = LocalTpsSpecType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(property);
        return Assert.IsAssignableFrom<IReadOnlyDictionary<string, TValue>>(property!.GetValue(null));
    }

    private static IReadOnlyDictionary<string, (string Accent, string Text, string Background)> ReadEmotionPalettes()
    {
        var property = LocalTpsSpecType.GetProperty(nameof(TpsSpec.EmotionPalettes), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(property);
        var palettes = Assert.IsAssignableFrom<System.Collections.IEnumerable>(property!.GetValue(null))
            .Cast<object>()
            .Select(item =>
            {
                var pairType = item.GetType();
                return (
                    Key: Assert.IsType<string>(pairType.GetProperty("Key")!.GetValue(item)),
                    Value: pairType.GetProperty("Value")!.GetValue(item)!);
            });

        return palettes.ToDictionary(
            pair => pair.Key,
            pair =>
            {
                var paletteType = pair.Value.GetType();
                return (
                    ReadPaletteMember(paletteType, pair.Value, "Accent"),
                    ReadPaletteMember(paletteType, pair.Value, "Text"),
                    ReadPaletteMember(paletteType, pair.Value, "Background"));
            },
            StringComparer.OrdinalIgnoreCase);
    }

    private static string ReadPaletteMember(Type paletteType, object palette, string memberName)
    {
        var property = paletteType.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(property);
        return Assert.IsType<string>(property!.GetValue(palette));
    }

    private static void AssertDictionariesEqual<TValue>(
        IReadOnlyDictionary<string, TValue> expected,
        IReadOnlyDictionary<string, TValue> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        foreach (var pair in expected)
        {
            Assert.True(actual.TryGetValue(pair.Key, out var actualValue), $"Missing key '{pair.Key}'.");
            Assert.Equal(pair.Value, actualValue);
        }
    }

    private static void AssertVocabularyEqual(IEnumerable<string> expected, IEnumerable<string> actual)
    {
        Assert.Equal(
            expected.OrderBy(value => value, StringComparer.OrdinalIgnoreCase),
            actual.OrderBy(value => value, StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
    }
}
