namespace PrompterLive.App.UITests;

internal static partial class BrowserTestConstants
{
    public static class Selectors
    {
        public const string CardCoverMeta = ".dcover-meta";
        public const string ClassAttribute = "class";
    }

    public static class EditorFlow
    {
        public const string OpeningBlock = "Opening Block";
        public const string PurposeBlock = "Purpose Block";
        public const string BenefitsBlock = "Benefits Block";
        public const string LearnSpeedAfterIncrease = "310";
        public const int BenefitsSegmentIndex = 2;
        public const int BenefitsBlockIndex = 1;
    }

    public static class SettingsFlow
    {
        public const string HtmlThemeAttribute = "data-theme";
        public const string LightTheme = "light";
        public const string OpenAiProviderId = "openai";
        public const string MicLevelPercentText = "82%";
        public const string MicLevelValue = "82";
        public const double NavItemLayoutTolerancePx = 0.5;
        public const string MicLevelInputScript =
            "element => { element.value = '82'; element.dispatchEvent(new Event('input', { bubbles: true })); }";
    }

    public static class TeleprompterFlow
    {
        public const string OpeningBlock = "Opening Block";
        public const string OpeningLine = "Good morning everyone";
        public const string CollapsedOpeningLine = "Goodmorningeveryone";
        public const string FontScaleAfterIncrease = "40";
        public const string WidthAfterChange = "900";
        public const string CameraRoleAttribute = "data-camera-role";
        public const string CameraDeviceIdAttribute = "data-camera-device-id";
        public const string CameraAutostartAttribute = "data-camera-autostart";
        public const string PrimaryCameraRole = "primary";
        public const string EnabledClass = "on";
        public const string FalseText = "false";
        public const string TrueText = "true";
        public const string WidthInputScript =
            "element => { element.value = '900'; element.dispatchEvent(new Event('input', { bubbles: true })); }";
        public const string HasVideoTrackScript =
            "element => !!element.srcObject && element.srcObject.getVideoTracks().length > 0";
        public const string ResolveCameraDeviceScript =
            """
            async () => {
                try {
                    const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
                    stream.getTracks().forEach(track => track.stop());
                } catch {
                }

                const devices = await navigator.mediaDevices.enumerateDevices();
                return devices.find(device => device.kind === 'videoinput')?.deviceId ?? 'default';
            }
            """;
        public const string SeedStoredSceneScript =
            """
            ({ cameraDeviceId }) => {
                localStorage.setItem('prompterlive.settings.prompterlive.reader', JSON.stringify({
                    CountdownSeconds: 3,
                    FontScale: 1,
                    TextWidth: 0.72,
                    ScrollSpeed: 1,
                    MirrorText: false,
                    ShowFocusLine: true,
                    ShowProgress: true,
                    ShowCameraScene: true
                }));

                localStorage.setItem('prompterlive.settings.prompterlive.scene', JSON.stringify({
                    Cameras: [
                        {
                            SourceId: 'scene-cam-a',
                            DeviceId: cameraDeviceId,
                            Label: 'Front camera',
                            Transform: {
                                X: 0.82,
                                Y: 0.82,
                                Width: 0.28,
                                Height: 0.28,
                                Rotation: 0,
                                MirrorHorizontal: true,
                                MirrorVertical: false,
                                Visible: true,
                                IncludeInOutput: true,
                                ZIndex: 1,
                                Opacity: 1
                            }
                        },
                        {
                            SourceId: 'scene-cam-b',
                            DeviceId: cameraDeviceId,
                            Label: 'Side camera',
                            Transform: {
                                X: 0.18,
                                Y: 0.18,
                                Width: 0.22,
                                Height: 0.22,
                                Rotation: 0,
                                MirrorHorizontal: false,
                                MirrorVertical: false,
                                Visible: true,
                                IncludeInOutput: true,
                                ZIndex: 2,
                                Opacity: 0.92
                            }
                        }
                    ],
                    PrimaryMicrophoneId: null,
                    PrimaryMicrophoneLabel: null,
                    AudioBus: {
                        Inputs: [],
                        MasterGain: 1,
                        MonitorEnabled: true
                    }
                }));

                localStorage.setItem('prompterlive.settings.prompterlive.studio', JSON.stringify({
                    Camera: {
                        DefaultCameraId: cameraDeviceId,
                        Resolution: 0,
                        MirrorCamera: true,
                        AutoStartOnRead: true
                    },
                    Microphone: {
                        DefaultMicrophoneId: null,
                        InputLevelPercent: 65,
                        NoiseSuppression: true,
                        EchoCancellation: true
                    },
                    Streaming: {
                        OutputMode: 0,
                        OutputResolution: 0,
                        BitrateKbps: 6000,
                        ShowTextOverlay: true,
                        IncludeCameraInOutput: true,
                        RtmpUrl: '',
                        StreamKey: ''
                    }
                }));
            }
            """;
    }
}
