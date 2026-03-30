namespace PrompterLive.App.UITests;

internal static partial class BrowserTestConstants
{
    public static class Media
    {
        public const string HarnessGlobal = "__prompterLiveMediaHarness";
        public const string PrimaryCameraId = "browser-cam-primary";
        public const string SecondaryCameraId = "browser-cam-secondary";
        public const string PrimaryCameraLabel = "Browser Camera A";
        public const string SecondaryCameraLabel = "Browser Camera B";
        public const string PrimaryMicrophoneId = "browser-mic-primary";
        public const string PrimaryMicrophoneLabel = "Browser Microphone";
        public const string VideoInputKind = "videoinput";
        public const string AudioInputKind = "audioinput";
        public const int ExpectedVideoTrackCount = 1;
        public const int ExpectedAudioTrackCount = 1;
        public const string ListDevicesScript = "() => window.__prompterLiveMediaHarness.listDevices()";
        public const string ClearRequestLogScript = "() => window.__prompterLiveMediaHarness.clearRequestLog()";
        public const string GetRequestLogScript = "() => window.__prompterLiveMediaHarness.getRequestLog()";
        public const string GetElementStateScript = "elementId => window.__prompterLiveMediaHarness.getElementState(elementId)";
        public const string ElementHasVideoStreamScript =
            "elementId => { const state = window.__prompterLiveMediaHarness.getElementState(elementId); return Boolean(state?.hasStream && state.videoTrackCount >= 1 && state.metadata?.isSynthetic === true); }";
        public const string ElementHasNoStreamScript =
            "elementId => { const state = window.__prompterLiveMediaHarness.getElementState(elementId); return Boolean(state && !state.hasStream); }";
        public const string ElementUsesVideoDeviceScript =
            "([elementId, deviceId]) => { const state = window.__prompterLiveMediaHarness.getElementState(elementId); return Boolean(state?.hasStream && state.metadata?.videoDeviceId === deviceId); }";
        public const string HasAudioVideoRequestScript =
            "([videoId, audioId]) => window.__prompterLiveMediaHarness.getRequestLog().some(request => request.hasVideo === true && request.hasAudio === true && request.resolvedVideoDeviceId === videoId && request.resolvedAudioDeviceId === audioId)";
        public const string HasVideoOnlyRequestScript =
            "([videoId]) => window.__prompterLiveMediaHarness.getRequestLog().some(request => request.hasVideo === true && request.hasAudio === false && request.resolvedVideoDeviceId === videoId)";
    }
}
