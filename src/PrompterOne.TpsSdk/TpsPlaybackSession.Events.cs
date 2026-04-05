using ManagedCode.Tps.Models;

namespace ManagedCode.Tps;

public sealed partial class TpsPlaybackSession
{
    private void Publish(Transition transition)
    {
        if (transition.StatusChangedRaised)
        {
            InvokeHandlers(TpsPlaybackEventNames.StatusChanged, StatusChanged, new TpsPlaybackStatusChangedEventArgs(transition.State, transition.PreviousStatus, transition.Status), transition.Snapshot);
        }

        if (transition.StateChangedRaised)
        {
            var args = new TpsPlaybackStateChangedEventArgs(transition.State, transition.PreviousState, transition.Status);
            InvokeHandlers(TpsPlaybackEventNames.StateChanged, StateChanged, args, transition.Snapshot);

            if (transition.WordChangedRaised)
            {
                InvokeHandlers(TpsPlaybackEventNames.WordChanged, WordChanged, args, transition.Snapshot);
            }

            if (transition.PhraseChangedRaised)
            {
                InvokeHandlers(TpsPlaybackEventNames.PhraseChanged, PhraseChanged, args, transition.Snapshot);
            }

            if (transition.BlockChangedRaised)
            {
                InvokeHandlers(TpsPlaybackEventNames.BlockChanged, BlockChanged, args, transition.Snapshot);
            }

            if (transition.SegmentChangedRaised)
            {
                InvokeHandlers(TpsPlaybackEventNames.SegmentChanged, SegmentChanged, args, transition.Snapshot);
            }

            if (transition.CompletedRaised)
            {
                InvokeHandlers(TpsPlaybackEventNames.Completed, Completed, args, transition.Snapshot);
            }
        }
        else if (transition.CompletedRaised)
        {
            InvokeHandlers(TpsPlaybackEventNames.Completed, Completed, new TpsPlaybackStateChangedEventArgs(transition.State, transition.PreviousState, transition.Status), transition.Snapshot);
        }

        InvokeHandlers(TpsPlaybackEventNames.SnapshotChanged, SnapshotChanged, new TpsPlaybackSnapshotChangedEventArgs(transition.Snapshot), transition.Snapshot);
    }

    private void InvokeHandlers<TEventArgs>(string eventName, EventHandler<TEventArgs>? handlers, TEventArgs args, TpsPlaybackSnapshot snapshot)
    {
        if (handlers is null)
        {
            return;
        }

        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler<TEventArgs>>())
        {
            Dispatch(() =>
            {
                try
                {
                    handler(this, args);
                }
                catch (Exception exception)
                {
                    ReportListenerException(exception, eventName, snapshot);
                }
            });
        }
    }

    private void InvokeSnapshotObserver(
        string eventName,
        Action<TpsPlaybackSnapshot> observer,
        TpsPlaybackSnapshot snapshot,
        SnapshotSubscription? subscription = null)
    {
        Dispatch(() =>
        {
            if (subscription?.IsDisposed == true)
            {
                return;
            }

            try
            {
                observer(snapshot);
            }
            catch (Exception exception)
            {
                ReportListenerException(exception, eventName, snapshot);
            }
        });
    }

    private void ReportListenerException(Exception exception, string eventName, TpsPlaybackSnapshot snapshot)
    {
        var handlers = ListenerException;
        if (handlers is null)
        {
            return;
        }

        var args = new TpsPlaybackListenerExceptionEventArgs(exception, eventName, snapshot, exception.Message);
        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler<TpsPlaybackListenerExceptionEventArgs>>())
        {
            Dispatch(() =>
            {
                try
                {
                    handler(this, args);
                }
                catch
                {
                }
            });
        }
    }

    private void Dispatch(Action callback)
    {
        if (_eventSynchronizationContext is null || ReferenceEquals(SynchronizationContext.Current, _eventSynchronizationContext))
        {
            callback();
            return;
        }

        _eventSynchronizationContext.Post(static state =>
        {
            ((Action)state!).Invoke();
        }, callback);
    }
}
