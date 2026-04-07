using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Services;

public static class GoLiveRemoteSourceInteropMethodNames
{
    private const string NamespacePrefix = AppMediaRuntime.GoLive.RemoteSourcesNamespace;

    public const string GetSessionState = NamespacePrefix + ".getSessionState";
    public const string StopSession = NamespacePrefix + ".stopSession";
    public const string SyncConnections = NamespacePrefix + ".syncConnections";
}
