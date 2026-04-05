using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public sealed partial class TpsPlaybackSession
{
    private sealed record Transition(
        PlayerState State,
        PlayerState PreviousState,
        TpsPlaybackStatus Status,
        TpsPlaybackStatus PreviousStatus,
        TpsPlaybackSnapshot Snapshot,
        bool StateChangedRaised,
        bool WordChangedRaised,
        bool PhraseChangedRaised,
        bool BlockChangedRaised,
        bool SegmentChangedRaised,
        bool StatusChangedRaised,
        bool CompletedRaised);

    private sealed class SnapshotSubscription(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;
        private int _disposed;

        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                Interlocked.Exchange(ref _dispose, null)?.Invoke();
            }
        }
    }
}
