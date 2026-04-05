using ManagedCode.Tps.Models;

namespace ManagedCode.Tps.Internal;

internal static partial class CompiledScriptValidator
{
    private static void ValidateWords(IReadOnlyList<CompiledWord> words, ISet<string> wordIds)
    {
        CompiledWord? previousWord = null;
        for (var index = 0; index < words.Count; index++)
        {
            var word = words[index];
            ValidateIdentifier(word.Id, "word", nameof(words), wordIds);

            if (word.Index != index)
            {
                throw new ArgumentException("Compiled TPS words must have sequential indexes that match their order.", nameof(words));
            }

            if (string.IsNullOrWhiteSpace(word.SegmentId) || string.IsNullOrWhiteSpace(word.BlockId))
            {
                throw new ArgumentException("Compiled TPS words must reference a segment and block.", nameof(words));
            }

            if (string.Equals(word.Kind, TpsSpec.WordKinds.Word, StringComparison.Ordinal) && string.IsNullOrWhiteSpace(word.PhraseId))
            {
                throw new ArgumentException("Compiled TPS spoken words must reference a phrase.", nameof(words));
            }

            if (word.StartMs < 0 || word.EndMs < word.StartMs)
            {
                throw new ArgumentException("Compiled TPS words must define a non-negative time range.", nameof(words));
            }

            if (word.EndMs - word.StartMs != word.DisplayDurationMs)
            {
                throw new ArgumentException("Compiled TPS words must keep display duration aligned with their start and end timestamps.", nameof(words));
            }

            if (previousWord is not null && word.StartMs != previousWord.EndMs)
            {
                throw new ArgumentException("Compiled TPS words must form a contiguous timeline.", nameof(words));
            }

            previousWord = word;
        }
    }

    private static void ValidateTimeRange(
        string scope,
        int startWordIndex,
        int endWordIndex,
        int startMs,
        int endMs,
        int wordCount,
        string parameterName)
    {
        if (startWordIndex < 0 || endWordIndex < startWordIndex || startMs < 0 || endMs < startMs)
        {
            throw new ArgumentException($"Compiled TPS {scope} ranges must be non-negative and ordered.", parameterName);
        }

        if (wordCount == 0)
        {
            if (startWordIndex != 0 || endWordIndex != 0 || startMs != 0 || endMs != 0)
            {
                throw new ArgumentException($"Compiled TPS empty {scope} ranges must stay at zero.", parameterName);
            }

            return;
        }

        if (startWordIndex >= wordCount || endWordIndex >= wordCount)
        {
            throw new ArgumentException($"Compiled TPS {scope} ranges must reference words inside the canonical timeline.", parameterName);
        }
    }

    private static void ValidateIdentifier(string id, string scope, string parameterName, ISet<string> seen)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException($"Compiled TPS {scope} identifiers cannot be empty.", parameterName);
        }

        if (!seen.Add(id))
        {
            throw new ArgumentException($"Compiled TPS {scope} identifiers must be unique.", parameterName);
        }
    }

    private static void ValidateWordReferences(
        IReadOnlyList<CompiledWord> words,
        IReadOnlySet<string> segmentIds,
        IReadOnlySet<string> blockIds,
        IReadOnlySet<string> phraseIds)
    {
        foreach (var word in words)
        {
            if (!segmentIds.Contains(word.SegmentId))
            {
                throw new ArgumentException($"Compiled TPS word '{word.Id}' references an unknown segment '{word.SegmentId}'.", nameof(words));
            }

            if (!blockIds.Contains(word.BlockId))
            {
                throw new ArgumentException($"Compiled TPS word '{word.Id}' references an unknown block '{word.BlockId}'.", nameof(words));
            }

            if (!string.IsNullOrWhiteSpace(word.PhraseId) && !phraseIds.Contains(word.PhraseId))
            {
                throw new ArgumentException($"Compiled TPS word '{word.Id}' references an unknown phrase '{word.PhraseId}'.", nameof(words));
            }
        }
    }

    private static void ValidateCanonicalScopeWords(
        string scope,
        string ownerId,
        IReadOnlyList<CompiledWord> scopeWords,
        int startWordIndex,
        int endWordIndex,
        int startMs,
        int endMs,
        IReadOnlyList<CompiledWord> canonicalWords,
        string parameterName,
        string expectedSegmentId,
        string? expectedBlockId = null,
        string? expectedPhraseId = null)
    {
        if (canonicalWords.Count == 0)
        {
            if (scopeWords.Count != 0)
            {
                throw new ArgumentException($"Compiled TPS {scope} '{ownerId}' cannot reference words when the canonical timeline is empty.", parameterName);
            }

            return;
        }

        if (scopeWords.Count == 0)
        {
            if (startWordIndex != 0 || endWordIndex != 0 || startMs != 0 || endMs != 0)
            {
                throw new ArgumentException($"Compiled TPS empty {scope} '{ownerId}' ranges must stay at zero.", parameterName);
            }

            return;
        }

        var expectedWordCount = endWordIndex - startWordIndex + 1;
        if (scopeWords.Count != expectedWordCount)
        {
            throw new ArgumentException($"Compiled TPS {scope} '{ownerId}' words must match the canonical range they claim to cover.", parameterName);
        }

        if (startMs != canonicalWords[startWordIndex].StartMs || endMs != canonicalWords[endWordIndex].EndMs)
        {
            throw new ArgumentException($"Compiled TPS {scope} '{ownerId}' timestamps must match the canonical word range they claim to cover.", parameterName);
        }

        for (var offset = 0; offset < scopeWords.Count; offset++)
        {
            var expectedWord = canonicalWords[startWordIndex + offset];
            var actualWord = scopeWords[offset];

            if (!string.Equals(actualWord.Id, expectedWord.Id, StringComparison.Ordinal) ||
                actualWord.Index != expectedWord.Index ||
                actualWord.StartMs != expectedWord.StartMs ||
                actualWord.EndMs != expectedWord.EndMs)
            {
                throw new ArgumentException($"Compiled TPS {scope} '{ownerId}' words must stay aligned with the canonical word timeline.", parameterName);
            }

            if (!string.Equals(actualWord.SegmentId, expectedSegmentId, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Compiled TPS {scope} '{ownerId}' words must reference segment '{expectedSegmentId}'.", parameterName);
            }

            if (expectedBlockId is not null && !string.Equals(actualWord.BlockId, expectedBlockId, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Compiled TPS {scope} '{ownerId}' words must reference block '{expectedBlockId}'.", parameterName);
            }

            if (expectedPhraseId is not null && !string.Equals(actualWord.PhraseId, expectedPhraseId, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Compiled TPS {scope} '{ownerId}' words must reference phrase '{expectedPhraseId}'.", parameterName);
            }
        }
    }
}
