using System.Text.Json.Serialization;

namespace ManagedCode.Tps.Models;

public sealed class CompiledBlock : ICompiledTimeRange
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("targetWpm")]
    public required int TargetWpm { get; init; }

    [JsonPropertyName("emotion")]
    public required string Emotion { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speaker")]
    public string? Speaker { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("archetype")]
    public string? Archetype { get; init; }

    [JsonPropertyName("isImplicit")]
    public required bool IsImplicit { get; init; }

    int ICompiledTimeRange.StartWordIndex
    {
        get => StartWordIndex;
        set => StartWordIndex = value;
    }

    [JsonInclude]
    [JsonPropertyName("startWordIndex")]
    public int StartWordIndex { get; internal set; }

    int ICompiledTimeRange.EndWordIndex
    {
        get => EndWordIndex;
        set => EndWordIndex = value;
    }

    [JsonInclude]
    [JsonPropertyName("endWordIndex")]
    public int EndWordIndex { get; internal set; }

    int ICompiledTimeRange.StartMs
    {
        get => StartMs;
        set => StartMs = value;
    }

    [JsonInclude]
    [JsonPropertyName("startMs")]
    public int StartMs { get; internal set; }

    int ICompiledTimeRange.EndMs
    {
        get => EndMs;
        set => EndMs = value;
    }

    [JsonInclude]
    [JsonPropertyName("endMs")]
    public int EndMs { get; internal set; }

    [JsonPropertyName("phrases")]
    public IReadOnlyList<CompiledPhrase> Phrases { get; init; } = Array.Empty<CompiledPhrase>();

    [JsonPropertyName("words")]
    public IReadOnlyList<CompiledWord> Words { get; init; } = Array.Empty<CompiledWord>();
}

public sealed class CompiledSegment : ICompiledTimeRange
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("targetWpm")]
    public required int TargetWpm { get; init; }

    [JsonPropertyName("emotion")]
    public required string Emotion { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speaker")]
    public string? Speaker { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("archetype")]
    public string? Archetype { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("timing")]
    public string? Timing { get; init; }

    [JsonPropertyName("backgroundColor")]
    public required string BackgroundColor { get; init; }

    [JsonPropertyName("textColor")]
    public required string TextColor { get; init; }

    [JsonPropertyName("accentColor")]
    public required string AccentColor { get; init; }

    int ICompiledTimeRange.StartWordIndex
    {
        get => StartWordIndex;
        set => StartWordIndex = value;
    }

    [JsonInclude]
    [JsonPropertyName("startWordIndex")]
    public int StartWordIndex { get; internal set; }

    int ICompiledTimeRange.EndWordIndex
    {
        get => EndWordIndex;
        set => EndWordIndex = value;
    }

    [JsonInclude]
    [JsonPropertyName("endWordIndex")]
    public int EndWordIndex { get; internal set; }

    int ICompiledTimeRange.StartMs
    {
        get => StartMs;
        set => StartMs = value;
    }

    [JsonInclude]
    [JsonPropertyName("startMs")]
    public int StartMs { get; internal set; }

    int ICompiledTimeRange.EndMs
    {
        get => EndMs;
        set => EndMs = value;
    }

    [JsonInclude]
    [JsonPropertyName("endMs")]
    public int EndMs { get; internal set; }

    [JsonPropertyName("blocks")]
    public IReadOnlyList<CompiledBlock> Blocks { get; init; } = Array.Empty<CompiledBlock>();

    [JsonPropertyName("words")]
    public IReadOnlyList<CompiledWord> Words { get; init; } = Array.Empty<CompiledWord>();
}

public sealed class CompiledScript
{
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    [JsonInclude]
    [JsonPropertyName("totalDurationMs")]
    public int TotalDurationMs { get; internal set; }

    [JsonPropertyName("segments")]
    public IReadOnlyList<CompiledSegment> Segments { get; init; } = Array.Empty<CompiledSegment>();

    [JsonPropertyName("words")]
    public IReadOnlyList<CompiledWord> Words { get; init; } = Array.Empty<CompiledWord>();
}
