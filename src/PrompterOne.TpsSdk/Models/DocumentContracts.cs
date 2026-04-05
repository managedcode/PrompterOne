using System.Text.Json.Serialization;

namespace ManagedCode.Tps.Models;

public sealed class TpsDocument
{
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    [JsonPropertyName("segments")]
    public IReadOnlyList<TpsSegment> Segments { get; init; } = Array.Empty<TpsSegment>();
}

public sealed class TpsSegment
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonInclude]
    [JsonPropertyName("content")]
    public string Content { get; internal set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("targetWpm")]
    public int? TargetWpm { get; init; }

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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    [JsonPropertyName("leadingContent")]
    public string? LeadingContent { get; internal set; }

    [JsonPropertyName("blocks")]
    public IReadOnlyList<TpsBlock> Blocks { get; init; } = Array.Empty<TpsBlock>();
}

public sealed class TpsBlock
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonInclude]
    [JsonPropertyName("content")]
    public string Content { get; internal set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("targetWpm")]
    public int? TargetWpm { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("emotion")]
    public string? Emotion { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speaker")]
    public string? Speaker { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("archetype")]
    public string? Archetype { get; init; }
}
