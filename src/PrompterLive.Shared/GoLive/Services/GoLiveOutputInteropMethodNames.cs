namespace PrompterLive.Shared.Services;

public static class GoLiveOutputInteropMethodNames
{
    private const string NamespacePrefix = "PrompterLiveGoLiveOutput";

    public const string GetSessionState = NamespacePrefix + ".getSessionState";
    public const string StartLocalRecording = NamespacePrefix + ".startLocalRecording";
    public const string StartLiveKitSession = NamespacePrefix + ".startLiveKitSession";
    public const string StartObsBrowserOutput = NamespacePrefix + ".startObsBrowserOutput";
    public const string StopLocalRecording = NamespacePrefix + ".stopLocalRecording";
    public const string StopLiveKitSession = NamespacePrefix + ".stopLiveKitSession";
    public const string StopObsBrowserOutput = NamespacePrefix + ".stopObsBrowserOutput";
    public const string UpdateSessionDevices = NamespacePrefix + ".updateSessionDevices";
}
