using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal static partial class CompiledScriptValidator
{
    public static void ValidateOrThrow(CompiledScript script)
    {
        ArgumentNullException.ThrowIfNull(script);

        if (script.TotalDurationMs < 0)
        {
            throw new ArgumentException("Compiled TPS script cannot have a negative total duration.", nameof(script));
        }

        if (script.Segments.Count == 0)
        {
            throw new ArgumentException("Compiled TPS script must contain at least one segment.", nameof(script));
        }

        var words = script.Words;
        if (words.Count == 0)
        {
            if (script.TotalDurationMs != 0)
            {
                throw new ArgumentException("Compiled TPS script with no words must have zero total duration.", nameof(script));
            }
        }
        else if (script.TotalDurationMs != words[^1].EndMs)
        {
            throw new ArgumentException("Compiled TPS script total duration must match the end of the final word.", nameof(script));
        }

        var segmentIds = new HashSet<string>(StringComparer.Ordinal);
        var blockIds = new HashSet<string>(StringComparer.Ordinal);
        var phraseIds = new HashSet<string>(StringComparer.Ordinal);
        var wordIds = new HashSet<string>(StringComparer.Ordinal);

        ValidateWords(words, wordIds);

        var expectedSegmentStartWordIndex = 0;
        foreach (var segment in script.Segments)
        {
            ValidateIdentifier(segment.Id, "segment", nameof(script), segmentIds);
            ValidateTimeRange("segment", segment.StartWordIndex, segment.EndWordIndex, segment.StartMs, segment.EndMs, words.Count, nameof(script));
            ValidateCanonicalScopeWords("segment", segment.Id, segment.Words, segment.StartWordIndex, segment.EndWordIndex, segment.StartMs, segment.EndMs, words, nameof(script), segment.Id);

            if (words.Count > 0 && segment.StartWordIndex != expectedSegmentStartWordIndex)
            {
                throw new ArgumentException("Compiled TPS segments must be ordered by their canonical timeline.", nameof(script));
            }

            if (words.Count > 0 && segment.Words.Count > 0 && segment.Blocks.Count == 0)
            {
                throw new ArgumentException("Compiled TPS segments with words must expose at least one block.", nameof(script));
            }

            var expectedBlockStartWordIndex = words.Count == 0 ? 0 : segment.StartWordIndex;
            foreach (var block in segment.Blocks)
            {
                ValidateBlock(block, segment, words, blockIds, phraseIds, ref expectedBlockStartWordIndex);
            }

            expectedSegmentStartWordIndex = segment.Words.Count == 0 ? segment.StartWordIndex : segment.EndWordIndex + 1;

            if (words.Count > 0 && segment.Blocks.Count > 0 && expectedBlockStartWordIndex != segment.EndWordIndex + 1)
            {
                throw new ArgumentException("Compiled TPS blocks must cover the full segment word timeline.", nameof(script));
            }
        }

        if (words.Count > 0 && expectedSegmentStartWordIndex != words.Count)
        {
            throw new ArgumentException("Compiled TPS segments do not cover the full word timeline.", nameof(script));
        }

        ValidateWordReferences(words, segmentIds, blockIds, phraseIds);
    }

    private static void ValidateBlock(
        CompiledBlock block,
        CompiledSegment segment,
        IReadOnlyList<CompiledWord> words,
        ISet<string> blockIds,
        ISet<string> phraseIds,
        ref int expectedBlockStartWordIndex)
    {
        ValidateIdentifier(block.Id, "block", nameof(words), blockIds);
        ValidateTimeRange("block", block.StartWordIndex, block.EndWordIndex, block.StartMs, block.EndMs, words.Count, nameof(words));
        ValidateCanonicalScopeWords("block", block.Id, block.Words, block.StartWordIndex, block.EndWordIndex, block.StartMs, block.EndMs, words, nameof(words), segment.Id, block.Id);

        if (block.Words.Count > 0 && (block.StartMs < segment.StartMs || block.EndMs > segment.EndMs))
        {
            throw new ArgumentException("Compiled TPS blocks must stay inside their parent segment range.", nameof(words));
        }

        if (words.Count > 0 && block.Words.Count > 0 && block.StartWordIndex != expectedBlockStartWordIndex)
        {
            throw new ArgumentException("Compiled TPS blocks must be ordered by their canonical timeline.", nameof(words));
        }

        var previousPhraseEndWordIndex = block.StartWordIndex - 1;
        foreach (var phrase in block.Phrases)
        {
            ValidatePhrase(phrase, block, segment.Id, words, phraseIds, ref previousPhraseEndWordIndex);
        }

        if (block.Words.Count > 0)
        {
            expectedBlockStartWordIndex = block.EndWordIndex + 1;
        }
    }

    private static void ValidatePhrase(
        CompiledPhrase phrase,
        CompiledBlock block,
        string segmentId,
        IReadOnlyList<CompiledWord> words,
        ISet<string> phraseIds,
        ref int previousPhraseEndWordIndex)
    {
        ValidateIdentifier(phrase.Id, "phrase", nameof(words), phraseIds);
        ValidateTimeRange("phrase", phrase.StartWordIndex, phrase.EndWordIndex, phrase.StartMs, phrase.EndMs, words.Count, nameof(words));
        if (phrase.Words.Count > 0 && (phrase.StartMs < block.StartMs || phrase.EndMs > block.EndMs))
        {
            throw new ArgumentException("Compiled TPS phrases must stay inside their parent block range.", nameof(words));
        }

        ValidateCanonicalScopeWords("phrase", phrase.Id, phrase.Words, phrase.StartWordIndex, phrase.EndWordIndex, phrase.StartMs, phrase.EndMs, words, nameof(words), segmentId, block.Id, phrase.Id);

        if (words.Count > 0 && phrase.Words.Count > 0 && phrase.StartWordIndex <= previousPhraseEndWordIndex)
        {
            throw new ArgumentException("Compiled TPS phrases must be ordered by their canonical timeline.", nameof(words));
        }

        if (phrase.Words.Count > 0)
        {
            previousPhraseEndWordIndex = phrase.EndWordIndex;
        }
    }
}
