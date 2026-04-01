namespace PrompterOne.Shared.Services;

internal sealed record GoLiveSessionSyncRequest
{
    public static GoLiveSessionSyncRequest Empty { get; } = new();
}
