namespace PrompterLive.App.UITests;

internal static class UiTestHostConstants
{
    public const string ApplicationMarker = "Prompter.live";
    public const string LoopbackBaseAddressTemplate = "http://127.0.0.1:0";
    public const int MaximumTcpPort = 65535;
    public const int MinimumDynamicPort = 1;
    public static readonly string[] GrantedPermissions = ["camera", "microphone"];
}
