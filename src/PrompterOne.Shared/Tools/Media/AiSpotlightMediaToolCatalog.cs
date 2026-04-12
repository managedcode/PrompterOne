using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightMediaToolCatalog
{
    public static void AddTo(List<AiSpotlightTool> tools)
    {
        AddReadOnly(tools, AiSpotlightToolNames.MediaDevicesList, UiTextKey.CommonVideo, AiSpotlightToolText.MediaDevicesList);
        AddReadOnly(tools, AiSpotlightToolNames.MediaPermissionsQuery, UiTextKey.CommonGeneral, AiSpotlightToolText.MediaPermissionsQuery);
        AddReadOnly(tools, AiSpotlightToolNames.MediaCapabilitiesRead, UiTextKey.CommonGeneral, AiSpotlightToolText.MediaCapabilitiesRead);
        AddReadOnly(tools, AiSpotlightToolNames.MediaCameraPreviewStateRead, UiTextKey.SettingsNavCameras, AiSpotlightToolText.MediaCameraPreviewStateRead);
        AddReadOnly(tools, AiSpotlightToolNames.MediaAudioOutputsList, UiTextKey.CommonAudio, AiSpotlightToolText.MediaAudioOutputsList);
        AddReadOnly(tools, AiSpotlightToolNames.MediaMicrophoneLevelsRead, UiTextKey.CommonAudio, AiSpotlightToolText.MediaMicrophoneLevelsRead, AiSpotlightToolParameterSets.MediaDevice);
        AddReadOnly(tools, AiSpotlightToolNames.MediaCameraPreviewAnalyze, UiTextKey.CommonVideo, AiSpotlightToolText.MediaCameraPreviewAnalyze, AiSpotlightToolParameterSets.MediaAnalysis, openWorld: true);
        AddReadOnly(tools, AiSpotlightToolNames.MediaMicrophoneAudioAnalyze, UiTextKey.CommonAudio, AiSpotlightToolText.MediaMicrophoneAudioAnalyze, AiSpotlightToolParameterSets.MediaAnalysis, openWorld: true);
        AddReadOnly(tools, AiSpotlightToolNames.MediaRecordingAnalyze, UiTextKey.CommonRecordings, AiSpotlightToolText.MediaRecordingAnalyze, AiSpotlightToolParameterSets.MediaAnalysis, openWorld: true);
        AddMutation(tools, AiSpotlightToolNames.MediaDevicesRefresh, UiTextKey.CommonGeneral, AiSpotlightToolText.MediaDevicesRefresh);
        AddSensitive(tools, AiSpotlightToolNames.MediaCameraAccessRequest, UiTextKey.SettingsNavCameras, AiSpotlightToolText.MediaCameraAccessRequest, openWorld: true);
        AddSensitive(tools, AiSpotlightToolNames.MediaMicrophoneAccessRequest, UiTextKey.SettingsNavMicrophones, AiSpotlightToolText.MediaMicrophoneAccessRequest, openWorld: true);
        AddMutation(tools, AiSpotlightToolNames.MediaCameraPreviewStart, UiTextKey.SettingsNavCameras, AiSpotlightToolText.MediaCameraPreviewStart, AiSpotlightToolParameterSets.MediaDevice);
        AddMutation(tools, AiSpotlightToolNames.MediaCameraPreviewStop, UiTextKey.SettingsNavCameras, AiSpotlightToolText.MediaCameraPreviewStop, AiSpotlightToolParameterSets.MediaDevice);
        AddMutation(tools, AiSpotlightToolNames.MediaCameraMirrorToggle, UiTextKey.SettingsCamerasMirrorHorizontal, AiSpotlightToolText.MediaCameraMirrorToggle, AiSpotlightToolParameterSets.MediaDevice, idempotent: false);
        AddMutation(tools, AiSpotlightToolNames.MediaMicrophoneMute, UiTextKey.CommonAudio, AiSpotlightToolText.MediaMicrophoneMute, AiSpotlightToolParameterSets.MediaDevice);
        AddMutation(tools, AiSpotlightToolNames.MediaMicrophoneUnmute, UiTextKey.CommonAudio, AiSpotlightToolText.MediaMicrophoneUnmute, AiSpotlightToolParameterSets.MediaDevice);
        AddMutation(tools, AiSpotlightToolNames.MediaMicrophoneGainUpdate, UiTextKey.SettingsMicrophonesInputLevel, AiSpotlightToolText.MediaMicrophoneGainUpdate, AiSpotlightToolParameterSets.MicrophoneGain);
        AddMutation(tools, AiSpotlightToolNames.MediaMicrophoneDelayUpdate, UiTextKey.SettingsMicrophonesAudioDelay, AiSpotlightToolText.MediaMicrophoneDelayUpdate, AiSpotlightToolParameterSets.MicrophoneDelay);
        AddMutation(tools, AiSpotlightToolNames.MediaAudioOutputSelect, UiTextKey.CommonAudio, AiSpotlightToolText.MediaAudioOutputSelect, AiSpotlightToolParameterSets.MediaDevice);
        AddSensitive(tools, AiSpotlightToolNames.MediaScreenCaptureRequest, UiTextKey.CommonVideo, AiSpotlightToolText.MediaScreenCaptureRequest, openWorld: true);
    }

    private static void AddReadOnly(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null,
        bool openWorld = false) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            label,
            UiTextKey.SettingsCamerasSectionDescription,
            prompt,
            AiSpotlightToolScopes.Media,
            parameters,
            openWorld: openWorld));

    private static void AddMutation(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null,
        bool openWorld = false,
        bool idempotent = true) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            label,
            UiTextKey.SettingsMicrophonesSectionDescription,
            prompt,
            AiSpotlightToolScopes.Media,
            parameters,
            readOnly: false,
            idempotent: idempotent,
            openWorld: openWorld));

    private static void AddSensitive(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null,
        bool openWorld = false) =>
        tools.Add(AiSpotlightToolFactory.SensitiveMutationTool(
            name,
            label,
            UiTextKey.SettingsMicrophonesSectionDescription,
            prompt,
            AiSpotlightToolScopes.Media,
            parameters,
            openWorld: openWorld));
}
