using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public sealed partial class TpsPlaybackSession
{
    public TpsPlaybackSnapshot CreateSnapshot()
    {
        ThrowIfDisposed();
        lock (_syncRoot)
        {
            return CreateSnapshotLocked();
        }
    }

    public void Dispose()
    {
        CancellationTokenSource? cancellationTokenSource;
        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            cancellationTokenSource = DetachLoopLocked();
            StateChanged = null;
            WordChanged = null;
            PhraseChanged = null;
            BlockChanged = null;
            SegmentChanged = null;
            StatusChanged = null;
            Completed = null;
            SnapshotChanged = null;
            ListenerException = null;
        }

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }

    private CancellationTokenSource? DetachLoop()
    {
        lock (_syncRoot)
        {
            return DetachLoopLocked();
        }
    }

    private CancellationTokenSource? DetachLoopLocked()
    {
        var current = _playbackCts;
        _playbackCts = null;
        return current;
    }

    private void ReplaceRunningLoop()
    {
        if (Status != TpsPlaybackStatus.Playing)
        {
            return;
        }

        var previousLoop = DetachLoop();
        previousLoop?.Cancel();
        previousLoop?.Dispose();
        var nextLoop = new CancellationTokenSource();
        lock (_syncRoot)
        {
            _playbackCts = nextLoop;
        }

        _ = RunPlaybackLoopAsync(nextLoop);
    }

    private async Task RunPlaybackLoopAsync(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_tickIntervalMs), _timeProvider, cancellationTokenSource.Token);

                Transition transition;
                lock (_syncRoot)
                {
                    if (!ReferenceEquals(_playbackCts, cancellationTokenSource) || Status != TpsPlaybackStatus.Playing)
                    {
                        break;
                    }

                    transition = UpdatePositionLocked(ReadLiveElapsedLocked(), TpsPlaybackStatus.Playing);
                    if (transition.CompletedRaised)
                    {
                        _playbackCts = null;
                    }
                }

                Publish(transition);
                if (transition.CompletedRaised)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (_syncRoot)
            {
                if (ReferenceEquals(_playbackCts, cancellationTokenSource))
                {
                    _playbackCts = null;
                }
            }

            cancellationTokenSource.Dispose();
        }
    }

    private int ReadLiveElapsedLocked()
    {
        var deltaMs = Math.Max(0d, _timeProvider.GetElapsedTime(_playbackStartedAtTimestamp).TotalMilliseconds);
        return _playbackOffsetMs + (int)Math.Max(0d, Math.Round(deltaMs * PlaybackRate, MidpointRounding.AwayFromZero));
    }

    private Transition UpdatePositionLocked(int elapsedMs, TpsPlaybackStatus requestedStatus)
    {
        var previousState = CurrentState;
        var previousStatus = Status;
        var nextState = Player.GetState(elapsedMs);
        var nextStatus = requestedStatus == TpsPlaybackStatus.Playing && nextState.IsComplete
            ? TpsPlaybackStatus.Completed
            : requestedStatus;

        CurrentState = nextState;
        Status = nextStatus;

        if (nextStatus != TpsPlaybackStatus.Playing)
        {
            _playbackOffsetMs = nextState.ElapsedMs;
            _playbackStartedAtTimestamp = 0;
        }

        return new Transition(
            nextState,
            previousState,
            nextStatus,
            previousStatus,
            CreateSnapshotLocked(),
            HasStateChanged(previousState, nextState),
            previousState.CurrentWord?.Id != nextState.CurrentWord?.Id,
            previousState.CurrentPhrase?.Id != nextState.CurrentPhrase?.Id,
            previousState.CurrentBlock?.Id != nextState.CurrentBlock?.Id,
            previousState.CurrentSegment?.Id != nextState.CurrentSegment?.Id,
            previousStatus != nextStatus,
            nextStatus == TpsPlaybackStatus.Completed && previousStatus != TpsPlaybackStatus.Completed);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static bool HasStateChanged(PlayerState previousState, PlayerState nextState)
    {
        return previousState.ElapsedMs != nextState.ElapsedMs
            || previousState.RemainingMs != nextState.RemainingMs
            || previousState.Progress != nextState.Progress
            || previousState.IsComplete != nextState.IsComplete
            || previousState.CurrentWord?.Id != nextState.CurrentWord?.Id
            || previousState.CurrentPhrase?.Id != nextState.CurrentPhrase?.Id
            || previousState.CurrentBlock?.Id != nextState.CurrentBlock?.Id
            || previousState.CurrentSegment?.Id != nextState.CurrentSegment?.Id;
    }

    private static TpsPlaybackStatus ResolveStatusAfterSeek(TpsPlaybackStatus previousStatus, int totalDurationMs, int elapsedMs)
    {
        if (totalDurationMs == 0 || elapsedMs >= totalDurationMs)
        {
            return TpsPlaybackStatus.Completed;
        }

        if (elapsedMs <= 0 && previousStatus == TpsPlaybackStatus.Idle)
        {
            return TpsPlaybackStatus.Idle;
        }

        return TpsPlaybackStatus.Paused;
    }
}
