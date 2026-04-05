using ManagedCode.Tps.Internal;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public sealed class TpsPlayer
{
    private readonly Dictionary<string, CompiledBlock> _blockById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CompiledPhrase> _phraseById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CompiledSegment> _segmentById = new(StringComparer.Ordinal);

    public TpsPlayer(CompiledScript script)
    {
        Script = CompiledScriptNormalizer.Normalize(script);
        foreach (var segment in Script.Segments)
        {
            _segmentById[segment.Id] = segment;
            foreach (var block in segment.Blocks)
            {
                _blockById[block.Id] = block;
                foreach (var phrase in block.Phrases)
                {
                    _phraseById[phrase.Id] = phrase;
                }
            }
        }
    }

    public CompiledScript Script { get; }

    public PlayerState GetState(int elapsedMs)
    {
        var clampedElapsed = Math.Clamp(elapsedMs, 0, Script.TotalDurationMs);
        var currentWord = FindCurrentWord(clampedElapsed);
        var currentSegment = currentWord is null
            ? Script.Segments.Count > 0 ? Script.Segments[0] : null
            : _segmentById.GetValueOrDefault(currentWord.SegmentId);
        var currentBlock = currentWord is null
            ? currentSegment is not null && currentSegment.Blocks.Count > 0 ? currentSegment.Blocks[0] : null
            : _blockById.GetValueOrDefault(currentWord.BlockId);
        var currentPhrase = currentWord is null
            ? currentBlock is not null && currentBlock.Phrases.Count > 0 ? currentBlock.Phrases[0] : null
            : _phraseById.GetValueOrDefault(currentWord.PhraseId);
        var currentWordIndex = currentWord?.Index ?? -1;
        var previousWord = currentWordIndex > 0 ? Script.Words[currentWordIndex - 1] : null;
        var nextWord = currentWordIndex >= 0 && currentWordIndex + 1 < Script.Words.Count
            ? Script.Words[currentWordIndex + 1]
            : null;

        return new PlayerState
        {
            ElapsedMs = clampedElapsed,
            RemainingMs = Math.Max(0, Script.TotalDurationMs - clampedElapsed),
            Progress = Script.TotalDurationMs == 0 ? 1d : clampedElapsed / (double)Script.TotalDurationMs,
            IsComplete = clampedElapsed >= Script.TotalDurationMs,
            CurrentWordIndex = currentWordIndex,
            CurrentWord = currentWord,
            PreviousWord = previousWord,
            NextWord = nextWord,
            CurrentSegment = currentSegment,
            CurrentBlock = currentBlock,
            CurrentPhrase = currentPhrase,
            NextTransitionMs = currentWord?.EndMs ?? Script.TotalDurationMs,
            Presentation = new PlayerPresentationModel
            {
                SegmentName = currentSegment?.Name,
                BlockName = currentBlock?.Name,
                PhraseText = currentPhrase?.Text,
                VisibleWords = currentPhrase?.Words ?? [],
                ActiveWordInPhrase = currentPhrase is null || currentWord is null
                    ? -1
                    : FindWordIndex(currentPhrase.Words, currentWord.Id)
            }
        };
    }

    public PlayerState Seek(int elapsedMs) => GetState(elapsedMs);

    public IEnumerable<PlayerState> EnumerateStates(int stepMs = 100)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stepMs);

        if (Script.TotalDurationMs == 0)
        {
            yield return GetState(0);
            yield break;
        }

        for (var elapsedMs = 0; elapsedMs < Script.TotalDurationMs; elapsedMs += stepMs)
        {
            yield return GetState(elapsedMs);
        }

        yield return GetState(Script.TotalDurationMs);
    }

    private CompiledWord? FindCurrentWord(int elapsedMs)
    {
        if (Script.Words.Count == 0)
        {
            return null;
        }

        var low = 0;
        var high = Script.Words.Count - 1;
        var candidateIndex = -1;

        while (low <= high)
        {
            var middle = low + ((high - low) / 2);
            var word = Script.Words[middle];
            if (word.EndMs > elapsedMs)
            {
                candidateIndex = middle;
                high = middle - 1;
            }
            else
            {
                low = middle + 1;
            }
        }

        if (candidateIndex >= 0)
        {
            for (var index = candidateIndex; index < Script.Words.Count; index++)
            {
                var word = Script.Words[index];
                if (word.EndMs > elapsedMs && word.EndMs > word.StartMs)
                {
                    return word;
                }
            }
        }

        return Script.Words[^1];
    }

    private static int FindWordIndex(IReadOnlyList<CompiledWord> words, string wordId)
    {
        for (var index = 0; index < words.Count; index++)
        {
            if (string.Equals(words[index].Id, wordId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}
