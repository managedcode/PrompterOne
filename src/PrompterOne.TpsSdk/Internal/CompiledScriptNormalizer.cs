using System.Collections.ObjectModel;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal static class CompiledScriptNormalizer
{
    public static CompiledScript Normalize(CompiledScript script)
    {
        CompiledScriptValidator.ValidateOrThrow(script);

        var normalizedWords = script.Words.Select(CloneWord).ToArray();
        var normalizedWordById = normalizedWords.ToDictionary(word => word.Id, StringComparer.Ordinal);
        var normalizedSegments = script.Segments.Select(segment => NormalizeSegment(segment, normalizedWordById)).ToArray();

        return new CompiledScript
        {
            Metadata = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(script.Metadata, StringComparer.OrdinalIgnoreCase)),
            TotalDurationMs = script.TotalDurationMs,
            Segments = Array.AsReadOnly(normalizedSegments),
            Words = Array.AsReadOnly(normalizedWords)
        };
    }

    private static CompiledSegment NormalizeSegment(
        CompiledSegment segment,
        IReadOnlyDictionary<string, CompiledWord> normalizedWordById)
    {
        var normalizedBlocks = segment.Blocks.Select(block => NormalizeBlock(block, normalizedWordById)).ToArray();
        var normalizedWords = segment.Words.Select(word => normalizedWordById[word.Id]).ToArray();

        return new CompiledSegment
        {
            Id = segment.Id,
            Name = segment.Name,
            TargetWpm = segment.TargetWpm,
            Emotion = segment.Emotion,
            Speaker = segment.Speaker,
            Archetype = segment.Archetype,
            Timing = segment.Timing,
            BackgroundColor = segment.BackgroundColor,
            TextColor = segment.TextColor,
            AccentColor = segment.AccentColor,
            StartWordIndex = segment.StartWordIndex,
            EndWordIndex = segment.EndWordIndex,
            StartMs = segment.StartMs,
            EndMs = segment.EndMs,
            Blocks = Array.AsReadOnly(normalizedBlocks),
            Words = Array.AsReadOnly(normalizedWords)
        };
    }

    private static CompiledBlock NormalizeBlock(
        CompiledBlock block,
        IReadOnlyDictionary<string, CompiledWord> normalizedWordById)
    {
        var normalizedPhrases = block.Phrases.Select(phrase => NormalizePhrase(phrase, normalizedWordById)).ToArray();
        var normalizedWords = block.Words.Select(word => normalizedWordById[word.Id]).ToArray();

        return new CompiledBlock
        {
            Id = block.Id,
            Name = block.Name,
            TargetWpm = block.TargetWpm,
            Emotion = block.Emotion,
            Speaker = block.Speaker,
            Archetype = block.Archetype,
            IsImplicit = block.IsImplicit,
            StartWordIndex = block.StartWordIndex,
            EndWordIndex = block.EndWordIndex,
            StartMs = block.StartMs,
            EndMs = block.EndMs,
            Phrases = Array.AsReadOnly(normalizedPhrases),
            Words = Array.AsReadOnly(normalizedWords)
        };
    }

    private static CompiledPhrase NormalizePhrase(
        CompiledPhrase phrase,
        IReadOnlyDictionary<string, CompiledWord> normalizedWordById)
    {
        var normalizedWords = phrase.Words.Select(word => normalizedWordById[word.Id]).ToArray();

        return new CompiledPhrase
        {
            Id = phrase.Id,
            Text = phrase.Text,
            StartWordIndex = phrase.StartWordIndex,
            EndWordIndex = phrase.EndWordIndex,
            StartMs = phrase.StartMs,
            EndMs = phrase.EndMs,
            Words = Array.AsReadOnly(normalizedWords)
        };
    }

    private static CompiledWord CloneWord(CompiledWord word) =>
        new()
        {
            Id = word.Id,
            Index = word.Index,
            Kind = word.Kind,
            CleanText = word.CleanText,
            CharacterCount = word.CharacterCount,
            OrpPosition = word.OrpPosition,
            DisplayDurationMs = word.DisplayDurationMs,
            StartMs = word.StartMs,
            EndMs = word.EndMs,
            Metadata = word.Metadata with { },
            SegmentId = word.SegmentId,
            BlockId = word.BlockId,
            PhraseId = word.PhraseId
        };
}
