namespace PrompterLive.Core.Models.Workspace;

public static class GoLiveTargetCatalog
{
    public static IReadOnlyList<string> AllTargetIds { get; } =
    [
        TargetIds.Obs,
        TargetIds.Ndi,
        TargetIds.Recording,
        TargetIds.LiveKit,
        TargetIds.VdoNinja,
        TargetIds.Youtube,
        TargetIds.Twitch,
        TargetIds.CustomRtmp
    ];

    public static class TargetIds
    {
        public const string Obs = "obs-studio";
        public const string Ndi = "ndi-output";
        public const string Recording = "local-recording";
        public const string LiveKit = "livekit";
        public const string VdoNinja = "vdoninja";
        public const string Youtube = "youtube-live";
        public const string Twitch = "twitch-live";
        public const string CustomRtmp = "custom-rtmp";
    }

    public static class TargetNames
    {
        public const string Obs = "OBS Studio";
        public const string Ndi = "NDI Output";
        public const string Recording = "Local Recording";
        public const string LiveKit = "LiveKit";
        public const string VdoNinja = "VDO.Ninja";
        public const string Youtube = "YouTube Live";
        public const string Twitch = "Twitch";
        public const string CustomRtmp = StreamingDefaults.CustomTargetName;
    }
}
