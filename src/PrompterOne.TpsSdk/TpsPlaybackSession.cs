using ManagedCode.Tps.Internal;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public sealed partial class TpsPlaybackSession : IDisposable
{
    private readonly Dictionary<string, int> _blockIndexById = new(StringComparer.Ordinal);
    private readonly List<CompiledBlock> _blocks;
    private readonly SynchronizationContext? _eventSynchronizationContext;
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, int> _segmentIndexById = new(StringComparer.Ordinal);
    private readonly int _tickIntervalMs;
    private readonly TimeProvider _timeProvider;
    private bool _disposed;
    private CancellationTokenSource? _playbackCts;
    private int _playbackOffsetMs;
    private long _playbackStartedAtTimestamp;
    private int _speedOffsetWpm;

    public TpsPlaybackSession(CompiledScript script, TpsPlaybackSessionOptions? options = null)
        : this(new TpsPlayer(script), options)
    {
    }

    public TpsPlaybackSession(TpsPlayer player, TpsPlaybackSessionOptions? options = null)
    {
        Player = player;
        var sessionOptions = options ?? new TpsPlaybackSessionOptions();
        _eventSynchronizationContext = sessionOptions.EventSynchronizationContext ?? SynchronizationContext.Current;
        _tickIntervalMs = sessionOptions.TickIntervalMs;
        _timeProvider = sessionOptions.TimeProvider;
        BaseWpm = NormalizeBaseWpm(sessionOptions.BaseWpm ?? TpsSupport.ResolveBaseWpm(player.Script.Metadata));
        SpeedStepWpm = sessionOptions.SpeedStepWpm;
        _speedOffsetWpm = NormalizeSpeedOffset(BaseWpm, sessionOptions.InitialSpeedOffsetWpm ?? 0);
        CurrentState = player.GetState(0);
        _blocks = FlattenBlocks(player.Script).ToList();

        for (var index = 0; index < player.Script.Segments.Count; index++)
        {
            _segmentIndexById[player.Script.Segments[index].Id] = index;
        }

        for (var index = 0; index < _blocks.Count; index++)
        {
            _blockIndexById[_blocks[index].Id] = index;
        }
    }

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? StateChanged;

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? WordChanged;

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? PhraseChanged;

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? BlockChanged;

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? SegmentChanged;

    public event EventHandler<TpsPlaybackStatusChangedEventArgs>? StatusChanged;

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? Completed;

    public event EventHandler<TpsPlaybackSnapshotChangedEventArgs>? SnapshotChanged;

    public event EventHandler<TpsPlaybackListenerExceptionEventArgs>? ListenerException;

    public int BaseWpm { get; }

    public PlayerState CurrentState { get; private set; }

    public int EffectiveBaseWpm => ClampWpm(BaseWpm + _speedOffsetWpm);

    public bool IsPlaying => Status == TpsPlaybackStatus.Playing;

    public TpsPlayer Player { get; }

    public double PlaybackRate => BaseWpm <= 0 ? 1d : EffectiveBaseWpm / (double)BaseWpm;

    public TpsPlaybackSnapshot Snapshot => CreateSnapshot();

    public int SpeedOffsetWpm => _speedOffsetWpm;

    public int SpeedStepWpm { get; }

    public TpsPlaybackStatus Status { get; private set; } = TpsPlaybackStatus.Idle;

    public IDisposable ObserveSnapshot(Action<TpsPlaybackSnapshot> observer, bool emitCurrent = true)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(observer);

        SnapshotSubscription? subscription = null;
        EventHandler<TpsPlaybackSnapshotChangedEventArgs> handler = (_, args) =>
        {
            if (subscription is { IsDisposed: false })
            {
                observer(args.Snapshot);
            }
        };

        subscription = new SnapshotSubscription(() => SnapshotChanged -= handler);
        SnapshotChanged += handler;

        if (emitCurrent)
        {
            InvokeSnapshotObserver(TpsPlaybackEventNames.ObserveSnapshot, observer, CreateSnapshot(), subscription);
        }

        return subscription;
    }

    public PlayerState Play()
    {
        ThrowIfDisposed();
        Transition? playTransition = null;
        Transition? completedTransition = null;
        CancellationTokenSource? previousLoop = null;
        CancellationTokenSource? nextLoop = null;
        var shouldStartLoop = false;

        lock (_syncRoot)
        {
            if (Status == TpsPlaybackStatus.Playing)
            {
                return CurrentState;
            }

            if (Player.Script.TotalDurationMs == 0)
            {
                completedTransition = UpdatePositionLocked(0, TpsPlaybackStatus.Completed);
            }
            else
            {
                previousLoop = DetachLoopLocked();
                nextLoop = new CancellationTokenSource();
                _playbackCts = nextLoop;
                var startElapsedMs = CurrentState.IsComplete ? 0 : CurrentState.ElapsedMs;
                _playbackOffsetMs = startElapsedMs;
                _playbackStartedAtTimestamp = _timeProvider.GetTimestamp();
                playTransition = UpdatePositionLocked(startElapsedMs, TpsPlaybackStatus.Playing);
                shouldStartLoop = playTransition.Status == TpsPlaybackStatus.Playing;
            }
        }

        previousLoop?.Cancel();
        previousLoop?.Dispose();

        if (completedTransition is not null)
        {
            Publish(completedTransition);
            return completedTransition.State;
        }

        if (!shouldStartLoop)
        {
            nextLoop?.Dispose();
        }

        Publish(playTransition!);
        if (shouldStartLoop)
        {
            _ = RunPlaybackLoopAsync(nextLoop!);
        }

        return playTransition!.State;
    }

    public PlayerState Pause()
    {
        ThrowIfDisposed();
        Transition transition;
        CancellationTokenSource? cancellationTokenSource;

        lock (_syncRoot)
        {
            if (Status != TpsPlaybackStatus.Playing)
            {
                return CurrentState;
            }

            cancellationTokenSource = DetachLoopLocked();
            transition = UpdatePositionLocked(ReadLiveElapsedLocked(), TpsPlaybackStatus.Paused);
        }

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        Publish(transition);
        return transition.State;
    }

    public PlayerState Stop()
    {
        ThrowIfDisposed();
        Transition transition;
        CancellationTokenSource? cancellationTokenSource;

        lock (_syncRoot)
        {
            cancellationTokenSource = DetachLoopLocked();
            _playbackOffsetMs = 0;
            _playbackStartedAtTimestamp = 0;
            transition = UpdatePositionLocked(0, TpsPlaybackStatus.Idle);
        }

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        Publish(transition);
        return transition.State;
    }

    public PlayerState Seek(int elapsedMs)
    {
        ThrowIfDisposed();
        Transition transition;
        CancellationTokenSource? cancellationTokenSource = null;

        lock (_syncRoot)
        {
            var requestedStatus = Status == TpsPlaybackStatus.Playing
                ? TpsPlaybackStatus.Playing
                : ResolveStatusAfterSeek(Status, Player.Script.TotalDurationMs, elapsedMs);

            transition = UpdatePositionLocked(elapsedMs, requestedStatus);
            if (transition.Status == TpsPlaybackStatus.Playing)
            {
                _playbackOffsetMs = transition.State.ElapsedMs;
                _playbackStartedAtTimestamp = _timeProvider.GetTimestamp();
            }
            else
            {
                cancellationTokenSource = DetachLoopLocked();
            }
        }

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        Publish(transition);
        return transition.State;
    }

    public PlayerState AdvanceBy(int deltaMs)
    {
        var anchorState = GetNavigationAnchorState();
        return Seek(checked(anchorState.ElapsedMs + deltaMs));
    }
}
