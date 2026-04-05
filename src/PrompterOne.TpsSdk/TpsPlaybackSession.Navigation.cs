using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public sealed partial class TpsPlaybackSession
{
    public PlayerState NextWord()
    {
        if (Player.Script.Words.Count == 0)
        {
            return CurrentState;
        }

        var anchorState = GetNavigationAnchorState();
        if (anchorState.CurrentWord is null)
        {
            return Seek(Player.Script.Words[0].StartMs);
        }

        var nextIndex = Math.Min(anchorState.CurrentWord.Index + 1, Player.Script.Words.Count - 1);
        return Seek(Player.Script.Words[nextIndex].StartMs);
    }

    public PlayerState PreviousWord()
    {
        if (Player.Script.Words.Count == 0)
        {
            return CurrentState;
        }

        var anchorState = GetNavigationAnchorState();
        var currentWord = anchorState.CurrentWord;
        if (currentWord is null)
        {
            return Seek(0);
        }

        if (anchorState.ElapsedMs > currentWord.StartMs)
        {
            return Seek(currentWord.StartMs);
        }

        var previousIndex = Math.Max(0, currentWord.Index - 1);
        return Seek(Player.Script.Words[previousIndex].StartMs);
    }

    public PlayerState NextBlock()
    {
        if (_blocks.Count == 0)
        {
            return CurrentState;
        }

        var anchorState = GetNavigationAnchorState();
        var currentIndex = anchorState.CurrentBlock is null
            ? -1
            : _blockIndexById.GetValueOrDefault(anchorState.CurrentBlock.Id, -1);
        var nextIndex = currentIndex < 0 ? 0 : Math.Min(currentIndex + 1, _blocks.Count - 1);
        return Seek(_blocks[nextIndex].StartMs);
    }

    public PlayerState PreviousBlock()
    {
        if (_blocks.Count == 0)
        {
            return CurrentState;
        }

        var anchorState = GetNavigationAnchorState();
        var currentBlock = anchorState.CurrentBlock;
        if (currentBlock is null)
        {
            return Seek(0);
        }

        var currentIndex = _blockIndexById.GetValueOrDefault(currentBlock.Id, 0);
        if (anchorState.ElapsedMs > currentBlock.StartMs)
        {
            return Seek(currentBlock.StartMs);
        }

        var previousIndex = Math.Max(0, currentIndex - 1);
        return Seek(_blocks[previousIndex].StartMs);
    }

    public TpsPlaybackSnapshot IncreaseSpeed(int? stepWpm = null)
    {
        return ChangeSpeedBy(stepWpm ?? SpeedStepWpm);
    }

    public TpsPlaybackSnapshot DecreaseSpeed(int? stepWpm = null)
    {
        return ChangeSpeedBy(-(stepWpm ?? SpeedStepWpm));
    }

    public TpsPlaybackSnapshot SetSpeedOffsetWpm(int offsetWpm)
    {
        ThrowIfDisposed();
        var normalized = NormalizeSpeedOffset(BaseWpm, offsetWpm);
        if (normalized == _speedOffsetWpm)
        {
            return Snapshot;
        }

        Transition transition;
        var restartLoop = false;

        lock (_syncRoot)
        {
            var elapsedMs = Status == TpsPlaybackStatus.Playing
                ? ReadLiveElapsedLocked()
                : CurrentState.ElapsedMs;

            _speedOffsetWpm = normalized;
            transition = UpdatePositionLocked(elapsedMs, Status);

            if (Status == TpsPlaybackStatus.Playing)
            {
                _playbackOffsetMs = transition.State.ElapsedMs;
                _playbackStartedAtTimestamp = _timeProvider.GetTimestamp();
                restartLoop = true;
            }
        }

        if (restartLoop)
        {
            ReplaceRunningLoop();
        }

        Publish(transition);
        return Snapshot;
    }

    private TpsPlaybackSnapshot ChangeSpeedBy(int deltaWpm)
    {
        return SetSpeedOffsetWpm(checked(_speedOffsetWpm + deltaWpm));
    }

    private PlayerState GetNavigationAnchorState()
    {
        lock (_syncRoot)
        {
            return Status == TpsPlaybackStatus.Playing
                ? Player.GetState(ReadLiveElapsedLocked())
                : CurrentState;
        }
    }

    private TpsPlaybackSnapshot CreateSnapshotLocked()
    {
        var state = CurrentState;
        var status = Status;
        var speedOffsetWpm = _speedOffsetWpm;
        var effectiveBaseWpm = ClampWpm(BaseWpm + speedOffsetWpm);
        var playbackRate = BaseWpm <= 0 ? 1d : effectiveBaseWpm / (double)BaseWpm;
        var visibleWords = (state.CurrentPhrase?.Words ?? Array.Empty<CompiledWord>())
            .Select(word => CreateWordView(word, state))
            .ToArray();
        var currentWord = state.CurrentWord;
        var currentSegmentIndex = state.CurrentSegment is null
            ? -1
            : _segmentIndexById.GetValueOrDefault(state.CurrentSegment.Id, -1);
        var currentBlockIndex = state.CurrentBlock is null
            ? -1
            : _blockIndexById.GetValueOrDefault(state.CurrentBlock.Id, -1);
        int? currentWordDurationMs = currentWord is null
            ? null
            : Math.Max(1, (int)Math.Round(currentWord.DisplayDurationMs / playbackRate, MidpointRounding.AwayFromZero));
        int? currentWordRemainingMs = currentWord is null
            ? null
            : Math.Max(0, (int)Math.Round((currentWord.EndMs - state.ElapsedMs) / playbackRate, MidpointRounding.AwayFromZero));

        return new TpsPlaybackSnapshot
        {
            Status = status,
            State = state,
            Tempo = new TpsPlaybackTempo(BaseWpm, effectiveBaseWpm, speedOffsetWpm, SpeedStepWpm, playbackRate),
            Controls = CreateControls(state, status, currentBlockIndex, effectiveBaseWpm),
            VisibleWords = visibleWords,
            FocusedWord = visibleWords.FirstOrDefault(word => word.IsActive),
            CurrentWordDurationMs = currentWordDurationMs,
            CurrentWordRemainingMs = currentWordRemainingMs,
            CurrentSegmentIndex = currentSegmentIndex,
            CurrentBlockIndex = currentBlockIndex
        };
    }

    private TpsPlaybackControls CreateControls(PlayerState state, TpsPlaybackStatus status, int currentBlockIndex, int effectiveBaseWpm)
    {
        var currentWordIndex = state.CurrentWordIndex;
        var canRewindCurrentWord = state.CurrentWord is not null && state.ElapsedMs > state.CurrentWord.StartMs;
        var canRewindCurrentBlock = state.CurrentBlock is not null && state.ElapsedMs > state.CurrentBlock.StartMs;
        return new TpsPlaybackControls(
            CanPlay: status != TpsPlaybackStatus.Playing,
            CanPause: status == TpsPlaybackStatus.Playing,
            CanStop: status != TpsPlaybackStatus.Idle || state.ElapsedMs > 0,
            CanNextWord: Player.Script.Words.Count > 0 && (state.CurrentWord is null || currentWordIndex < Player.Script.Words.Count - 1),
            CanPreviousWord: Player.Script.Words.Count > 0 && (currentWordIndex > 0 || canRewindCurrentWord),
            CanNextBlock: _blocks.Count > 0 && (state.CurrentBlock is null || currentBlockIndex < _blocks.Count - 1),
            CanPreviousBlock: _blocks.Count > 0 && (currentBlockIndex > 0 || canRewindCurrentBlock),
            CanIncreaseSpeed: effectiveBaseWpm < TpsSpec.MaximumWpm,
            CanDecreaseSpeed: effectiveBaseWpm > TpsSpec.MinimumWpm);
    }

    private static TpsPlaybackWordView CreateWordView(CompiledWord word, PlayerState state)
    {
        return new TpsPlaybackWordView
        {
            Word = word,
            IsActive = word.Id == state.CurrentWord?.Id,
            IsRead = word.EndMs <= state.ElapsedMs,
            IsUpcoming = word.StartMs > state.ElapsedMs,
            Emotion = word.Metadata.InlineEmotionHint
                ?? word.Metadata.EmotionHint
                ?? state.CurrentBlock?.Emotion
                ?? state.CurrentSegment?.Emotion
                ?? TpsSpec.DefaultEmotion,
            Speaker = word.Metadata.Speaker ?? state.CurrentBlock?.Speaker ?? state.CurrentSegment?.Speaker,
            EmphasisLevel = word.Metadata.EmphasisLevel,
            IsHighlighted = word.Metadata.IsHighlight,
            DeliveryMode = word.Metadata.DeliveryMode,
            VolumeLevel = word.Metadata.VolumeLevel
        };
    }

    private static IEnumerable<CompiledBlock> FlattenBlocks(CompiledScript script)
    {
        foreach (var segment in script.Segments)
        {
            foreach (var block in segment.Blocks)
            {
                yield return block;
            }
        }
    }

    private static int NormalizeBaseWpm(int value)
    {
        return ClampWpm(value);
    }

    private static int NormalizeSpeedOffset(int baseWpm, int offsetWpm)
    {
        return ClampWpm(baseWpm + offsetWpm) - baseWpm;
    }

    private static int ClampWpm(int value)
    {
        return Math.Min(Math.Max(value, TpsSpec.MinimumWpm), TpsSpec.MaximumWpm);
    }
}
