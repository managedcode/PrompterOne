using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightSettingsToolCatalog
{
    public static void AddTo(List<AiSpotlightTool> tools)
    {
        AddNavigationSettings(tools);
        AddSettingsMutations(tools);
    }

    private static void AddNavigationSettings(List<AiSpotlightTool> tools)
    {
        AddOpen(tools, AiSpotlightToolNames.SettingsCloud, UiTextKey.SettingsNavCloud, UiTextKey.SettingsCloudSectionDescription, AiSpotlightToolText.OpenCloudSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsFiles, UiTextKey.SettingsNavFiles, UiTextKey.SettingsFilesSectionDescription, AiSpotlightToolText.OpenFilesSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsCameras, UiTextKey.SettingsNavCameras, UiTextKey.SettingsCamerasSectionDescription, AiSpotlightToolText.OpenCamerasSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsMicrophones, UiTextKey.SettingsNavMicrophones, UiTextKey.SettingsMicrophonesSectionDescription, AiSpotlightToolText.OpenMicrophonesSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsStreaming, UiTextKey.SettingsNavStreaming, UiTextKey.SettingsStreamingSectionDescription, AiSpotlightToolText.OpenStreamingSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsRecording, UiTextKey.SettingsNavRecording, UiTextKey.SettingsRecordingSectionDescription, AiSpotlightToolText.OpenRecordingSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsAi, UiTextKey.SettingsNavAi, UiTextKey.SettingsAiSectionDescription, AiSpotlightToolText.OpenAiSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsAppearance, UiTextKey.SettingsNavAppearance, UiTextKey.SettingsAppearanceSectionDescription, AiSpotlightToolText.OpenAppearanceSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsLanguage, UiTextKey.SettingsNavLanguage, UiTextKey.SettingsLanguageSectionDescription, AiSpotlightToolText.OpenLanguageSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsShortcuts, UiTextKey.SettingsNavShortcuts, UiTextKey.SettingsShortcutsSectionDescription, AiSpotlightToolText.OpenShortcutsSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsFeedback, UiTextKey.SettingsNavFeedback, UiTextKey.SettingsFeedbackUnavailableNote, AiSpotlightToolText.OpenFeedbackSettings);
        AddOpen(tools, AiSpotlightToolNames.SettingsAbout, UiTextKey.SettingsNavAbout, UiTextKey.AiSpotlightSuggestionOpenSettingsDetail, AiSpotlightToolText.OpenAboutSettings);
    }

    private static void AddSettingsMutations(List<AiSpotlightTool> tools)
    {
        AddReadOnly(tools, AiSpotlightToolNames.SettingsSectionsList, AiSpotlightToolText.SettingsSectionsList);
        AddReadOnly(tools, AiSpotlightToolNames.SettingRead, AiSpotlightToolText.SettingRead);
        AddReadOnly(tools, AiSpotlightToolNames.SettingsMediaDefaultsRead, AiSpotlightToolText.SettingsMediaDefaultsRead);
        AddReadOnly(tools, AiSpotlightToolNames.SettingsRecordingProfileRead, AiSpotlightToolText.SettingsRecordingProfileRead);
        AddMutation(tools, AiSpotlightToolNames.SettingUpdate, AiSpotlightToolText.SettingUpdate, AiSpotlightToolParameterSets.SettingValue);
        AddMutation(tools, AiSpotlightToolNames.SettingReset, AiSpotlightToolText.SettingReset, AiSpotlightToolParameterSets.SettingValue);
        AddMutation(tools, AiSpotlightToolNames.SettingsDefaultCameraSet, AiSpotlightToolText.SettingsDefaultCameraSet, AiSpotlightToolParameterSets.DeviceSelection);
        AddMutation(tools, AiSpotlightToolNames.SettingsDefaultMicrophoneSet, AiSpotlightToolText.SettingsDefaultMicrophoneSet, AiSpotlightToolParameterSets.DeviceSelection);
        AddMutation(tools, AiSpotlightToolNames.SettingsMicrophoneEnable, AiSpotlightToolText.SettingsMicrophoneEnable, AiSpotlightToolParameterSets.DeviceSelection);
        AddMutation(tools, AiSpotlightToolNames.SettingsMicrophoneDisable, AiSpotlightToolText.SettingsMicrophoneDisable, AiSpotlightToolParameterSets.DeviceSelection);
        AddMutation(tools, AiSpotlightToolNames.SettingsRecordingProfileUpdate, AiSpotlightToolText.SettingsRecordingProfileUpdate, AiSpotlightToolParameterSets.SettingValue);
        AddSensitive(tools, AiSpotlightToolNames.SettingsAiSecretUpdate, AiSpotlightToolText.SettingsAiSecretUpdate);
        AddSensitive(tools, AiSpotlightToolNames.SettingsCloudSecretUpdate, AiSpotlightToolText.SettingsCloudSecretUpdate);
    }

    private static void AddOpen(
        List<AiSpotlightTool> tools,
        string name,
        UiTextKey label,
        UiTextKey detail,
        string prompt) =>
        tools.Add(AiSpotlightToolFactory.NavigationTool(
            name,
            label,
            detail,
            prompt,
            AppRoutes.Settings,
            AiSpotlightToolScopes.Settings));

    private static void AddReadOnly(List<AiSpotlightTool> tools, string name, string prompt) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            UiTextKey.SettingsTitle,
            UiTextKey.AiSpotlightSuggestionOpenSettingsDetail,
            prompt,
            AiSpotlightToolScopes.Settings));

    private static void AddMutation(
        List<AiSpotlightTool> tools,
        string name,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter> parameters) =>
        tools.Add(AiSpotlightToolFactory.SettingsTool(
            name,
            UiTextKey.SettingsTitle,
            UiTextKey.AiSpotlightSuggestionOpenSettingsDetail,
            prompt,
            parameters));

    private static void AddSensitive(List<AiSpotlightTool> tools, string name, string prompt) =>
        tools.Add(AiSpotlightToolFactory.SensitiveMutationTool(
            name,
            UiTextKey.SettingsTitle,
            UiTextKey.AiSpotlightSuggestionOpenSettingsDetail,
            prompt,
            AiSpotlightToolScopes.Settings,
            AiSpotlightToolParameterSets.SettingValue,
            openWorld: true));
}
