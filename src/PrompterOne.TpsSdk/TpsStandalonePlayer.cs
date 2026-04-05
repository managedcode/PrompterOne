using System.Collections.ObjectModel;
using System.Text.Json;
using ManagedCode.Tps.Internal;
using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public sealed class TpsStandalonePlayer : IDisposable
{
    public TpsStandalonePlayer(TpsCompilationResult compilation, TpsStandalonePlayerOptions? options = null)
        : this(compilation, new TpsPlaybackSession(compilation.Script, options), options, hasSourceCompilation: true)
    {
    }

    public TpsStandalonePlayer(CompiledScript script, TpsStandalonePlayerOptions? options = null)
        : this(CreateStandaloneProjection(script), options)
    {
    }

    private TpsStandalonePlayer(StandaloneProjection projection, TpsStandalonePlayerOptions? options)
        : this(projection.Compilation, new TpsPlaybackSession(projection.Script, options), options, hasSourceCompilation: false)
    {
    }

    private TpsStandalonePlayer(
        TpsCompilationResult compilation,
        TpsPlaybackSession session,
        TpsStandalonePlayerOptions? options,
        bool hasSourceCompilation)
    {
        Compilation = compilation;
        HasSourceCompilation = hasSourceCompilation;
        Session = session;
        if (options?.AutoPlay == true)
        {
            ScheduleAutoPlay(options);
        }
    }

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? StateChanged
    {
        add => Session.StateChanged += value;
        remove => Session.StateChanged -= value;
    }

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? WordChanged
    {
        add => Session.WordChanged += value;
        remove => Session.WordChanged -= value;
    }

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? PhraseChanged
    {
        add => Session.PhraseChanged += value;
        remove => Session.PhraseChanged -= value;
    }

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? BlockChanged
    {
        add => Session.BlockChanged += value;
        remove => Session.BlockChanged -= value;
    }

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? SegmentChanged
    {
        add => Session.SegmentChanged += value;
        remove => Session.SegmentChanged -= value;
    }

    public event EventHandler<TpsPlaybackStatusChangedEventArgs>? StatusChanged
    {
        add => Session.StatusChanged += value;
        remove => Session.StatusChanged -= value;
    }

    public event EventHandler<TpsPlaybackStateChangedEventArgs>? Completed
    {
        add => Session.Completed += value;
        remove => Session.Completed -= value;
    }

    public event EventHandler<TpsPlaybackSnapshotChangedEventArgs>? SnapshotChanged
    {
        add => Session.SnapshotChanged += value;
        remove => Session.SnapshotChanged -= value;
    }

    public event EventHandler<TpsPlaybackListenerExceptionEventArgs>? ListenerException
    {
        add => Session.ListenerException += value;
        remove => Session.ListenerException -= value;
    }

    public TpsCompilationResult Compilation { get; }

    public PlayerState CurrentState => Session.CurrentState;

    public IReadOnlyList<TpsDiagnostic> Diagnostics => Compilation.Diagnostics;

    public TpsDocument Document => Compilation.Document;

    public bool HasProjectedDocument => !HasSourceCompilation;

    public bool HasSourceCompilation { get; }

    public bool IsPlaying => Session.IsPlaying;

    public bool Ok => Compilation.Ok;

    public TpsPlaybackSession Session { get; }

    public CompiledScript Script => Compilation.Script;

    public TpsPlaybackSnapshot Snapshot => Session.Snapshot;

    public TpsPlaybackStatus Status => Session.Status;

    public IDisposable ObserveSnapshot(Action<TpsPlaybackSnapshot> observer, bool emitCurrent = true) =>
        Session.ObserveSnapshot(observer, emitCurrent);

    public static TpsStandalonePlayer Compile(string source, TpsStandalonePlayerOptions? options = null)
    {
        return new TpsStandalonePlayer(TpsRuntime.Compile(source), options);
    }

    public static TpsStandalonePlayer FromCompiledScript(CompiledScript script, TpsStandalonePlayerOptions? options = null)
    {
        return new TpsStandalonePlayer(script, options);
    }

    public static TpsStandalonePlayer FromCompiledJson(
        string json,
        TpsStandalonePlayerOptions? options = null,
        JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var script = JsonSerializer.Deserialize<CompiledScript>(json, serializerOptions)
            ?? throw new JsonException("Compiled TPS JSON did not produce a script instance.");
        return new TpsStandalonePlayer(script, options);
    }

    public PlayerState Play() => Session.Play();

    public PlayerState Pause() => Session.Pause();

    public PlayerState Stop() => Session.Stop();

    public PlayerState Seek(int elapsedMs) => Session.Seek(elapsedMs);

    public PlayerState AdvanceBy(int deltaMs) => Session.AdvanceBy(deltaMs);

    public PlayerState NextWord() => Session.NextWord();

    public PlayerState PreviousWord() => Session.PreviousWord();

    public PlayerState NextBlock() => Session.NextBlock();

    public PlayerState PreviousBlock() => Session.PreviousBlock();

    public TpsPlaybackSnapshot IncreaseSpeed(int? stepWpm = null) => Session.IncreaseSpeed(stepWpm);

    public TpsPlaybackSnapshot DecreaseSpeed(int? stepWpm = null) => Session.DecreaseSpeed(stepWpm);

    public TpsPlaybackSnapshot SetSpeedOffsetWpm(int offsetWpm) => Session.SetSpeedOffsetWpm(offsetWpm);

    private static TpsCompilationResult CreateProjectedCompilation(CompiledScript script)
    {
        ArgumentNullException.ThrowIfNull(script);

        var projectedSegments = script.Segments.Select(segment => new TpsSegment
        {
            Id = segment.Id,
            Name = segment.Name,
            Content = string.Empty,
            TargetWpm = segment.TargetWpm,
            Emotion = segment.Emotion,
            Speaker = segment.Speaker,
            Timing = segment.Timing,
            BackgroundColor = segment.BackgroundColor,
            TextColor = segment.TextColor,
            AccentColor = segment.AccentColor,
            LeadingContent = null,
            Blocks = Array.AsReadOnly(segment.Blocks.Select(block => new TpsBlock
            {
                Id = block.Id,
                Name = block.Name,
                Content = string.Empty,
                TargetWpm = block.TargetWpm,
                Emotion = block.Emotion,
                Speaker = block.Speaker
            }).ToArray())
        }).ToArray();

        return new TpsCompilationResult
        {
            Ok = true,
            Diagnostics = [],
            Document = new TpsDocument
            {
                Metadata = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(script.Metadata, StringComparer.OrdinalIgnoreCase)),
                Segments = Array.AsReadOnly(projectedSegments)
            },
            Script = script
        };
    }

    private static StandaloneProjection CreateStandaloneProjection(CompiledScript script)
    {
        var normalizedScript = CompiledScriptNormalizer.Normalize(script);
        return new StandaloneProjection(CreateProjectedCompilation(normalizedScript), normalizedScript);
    }

    private void ScheduleAutoPlay(TpsStandalonePlayerOptions options)
    {
        var synchronizationContext = options.EventSynchronizationContext ?? SynchronizationContext.Current;
        if (synchronizationContext is not null)
        {
            synchronizationContext.Post(static state => ((TpsStandalonePlayer)state!).TryAutoPlay(), this);
            return;
        }

        ThreadPool.UnsafeQueueUserWorkItem(static state => ((TpsStandalonePlayer)state!).TryAutoPlay(), this);
    }

    private void TryAutoPlay()
    {
        try
        {
            if (!Session.IsPlaying)
            {
                Session.Play();
            }
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public void Dispose()
    {
        Session.Dispose();
    }

    private sealed record StandaloneProjection(TpsCompilationResult Compilation, CompiledScript Script);
}
