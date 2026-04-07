namespace PrompterOne.Shared.Contracts;

public static class AppMediaRuntime
{
    public static class Runtime
    {
        public const string ContractProperty = "media";
        public const string GlobalName = AppRuntimeTelemetry.Harness.RuntimeGlobalName;
        public const string HarnessEnabledProperty = "mediaHarnessEnabled";
    }

    public static class BrowserMedia
    {
        public const string CaptureCapabilitiesOverrideGlobalName = "__prompterOneMediaCapabilityOverride";
        public const string ConcealDeviceIdentitySessionFlag = "__prompterOneConcealDeviceIdentityUntilMediaRequest";
        public const string InteropNamespace = "BrowserMediaInterop";
        public const string RecordingFileHarnessGlobalName = "__prompterOneRecordingFileHarness";
        public const string RemoteSourceSeedGlobalName = "__prompterOneRemoteSourceSeed";
        public const string SyntheticHarnessGlobalName = "__prompterOneMediaHarness";
        public const string SyntheticMetadataProperty = "__prompterOneSyntheticMedia";
    }

    public static class GoLive
    {
        public const string CleanupHarnessGlobalName = "__prompterOneGoLiveCleanupHarness";
        public const string DefaultVdoNinjaBaseUrl = "https://vdo.ninja/";
        public const string DefaultVdoNinjaStreamLabel = "PrompterOne Program";
        public const string LiveKitHarnessGlobalName = "__prompterOneLiveKitHarness";
        public const string MediaComposerNamespace = "PrompterOneGoLiveMediaComposer";
        public const string OutputNamespace = "PrompterOneGoLiveOutput";
        public const string OutputSupportNamespace = "PrompterOneGoLiveOutputSupport";
        public const string OutputVdoNinjaNamespace = "PrompterOneGoLiveOutputVdoNinja";
        public const string RemoteSourcesNamespace = "PrompterOneGoLiveRemoteSources";
        public const string VdoNinjaHarnessGlobalName = "__prompterOneVdoNinjaHarness";
    }

    public static class Vendor
    {
        public const string LiveKitClientGlobalName = "LivekitClient";
        public const string VdoNinjaLegacyGlobalName = "VDONinja";
        public const string VdoNinjaSdkGlobalName = "VDONinjaSDK";
    }
}
