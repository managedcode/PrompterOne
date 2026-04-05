using System.Text.Json.Serialization;

namespace ManagedCode.Tps.Models;

public sealed class PlayerPresentationModel
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("segmentName")]
    public string? SegmentName { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("blockName")]
    public string? BlockName { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("phraseText")]
    public string? PhraseText { get; init; }

    [JsonPropertyName("visibleWords")]
    public required IReadOnlyList<CompiledWord> VisibleWords { get; init; }

    [JsonPropertyName("activeWordInPhrase")]
    public required int ActiveWordInPhrase { get; init; }
}

public sealed class PlayerState
{
    [JsonPropertyName("elapsedMs")]
    public required int ElapsedMs { get; init; }

    [JsonPropertyName("remainingMs")]
    public required int RemainingMs { get; init; }

    [JsonPropertyName("progress")]
    public required double Progress { get; init; }

    [JsonPropertyName("isComplete")]
    public required bool IsComplete { get; init; }

    [JsonPropertyName("currentWordIndex")]
    public required int CurrentWordIndex { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentWord")]
    public CompiledWord? CurrentWord { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("previousWord")]
    public CompiledWord? PreviousWord { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("nextWord")]
    public CompiledWord? NextWord { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentSegment")]
    public CompiledSegment? CurrentSegment { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentBlock")]
    public CompiledBlock? CurrentBlock { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("currentPhrase")]
    public CompiledPhrase? CurrentPhrase { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("nextTransitionMs")]
    public int? NextTransitionMs { get; init; }

    [JsonPropertyName("presentation")]
    public required PlayerPresentationModel Presentation { get; init; }
}
