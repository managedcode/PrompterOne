namespace PrompterOne.Shared.Services;

internal static class CrossTabMessagingDefaults
{
    public const string ChannelName = "prompterone.cross-tab.v1";
    public const string ReceiveMethodName = nameof(CrossTabMessageBus.ReceiveAsync);
}
