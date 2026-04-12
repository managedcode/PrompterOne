using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightStreamingToolCatalog
{
    public static void AddTo(List<AiSpotlightTool> tools)
    {
        AddReadOnly(tools, AiSpotlightToolNames.StreamGoLiveStateRead, AiSpotlightToolText.StreamGoLiveStateRead);
        AddReadOnly(tools, AiSpotlightToolNames.StreamTransportsList, AiSpotlightToolText.StreamTransportsList);
        AddReadOnly(tools, AiSpotlightToolNames.StreamTargetsList, AiSpotlightToolText.StreamTargetsList);
        AddReadOnly(tools, AiSpotlightToolNames.StreamTransportHealthRead, AiSpotlightToolText.StreamTransportHealthRead);
        AddReadOnly(tools, AiSpotlightToolNames.StreamRecordingArtifactsList, AiSpotlightToolText.StreamRecordingArtifactsList);
        AddReadOnly(tools, AiSpotlightToolNames.StreamScenesList, AiSpotlightToolText.StreamScenesList);
        AddReadOnly(tools, AiSpotlightToolNames.StreamTargetValidate, AiSpotlightToolText.StreamTargetValidate, AiSpotlightToolParameterSets.StreamTarget, openWorld: true);
        AddReadOnly(tools, AiSpotlightToolNames.StreamHealthAnalyze, AiSpotlightToolText.StreamHealthAnalyze, AiSpotlightToolParameterSets.Source, openWorld: true);
        AddReadOnly(tools, AiSpotlightToolNames.StreamProgramVideoAnalyze, AiSpotlightToolText.StreamProgramVideoAnalyze, AiSpotlightToolParameterSets.Source, openWorld: true);
        AddReadOnly(tools, AiSpotlightToolNames.StreamProgramAudioAnalyze, AiSpotlightToolText.StreamProgramAudioAnalyze, AiSpotlightToolParameterSets.Source, openWorld: true);
        AddMutation(tools, AiSpotlightToolNames.StreamLiveKitTransportConfigure, AiSpotlightToolText.StreamLiveKitTransportConfigure, AiSpotlightToolParameterSets.Transport, openWorld: true);
        AddMutation(tools, AiSpotlightToolNames.StreamVdoNinjaTransportConfigure, AiSpotlightToolText.StreamVdoNinjaTransportConfigure, AiSpotlightToolParameterSets.Transport, openWorld: true);
        AddSensitive(tools, AiSpotlightToolNames.StreamYouTubeKeyConfigure, AiSpotlightToolText.StreamYouTubeKeyConfigure, AiSpotlightToolParameterSets.StreamTarget, openWorld: true);
        AddSensitive(tools, AiSpotlightToolNames.StreamTwitchKeyConfigure, AiSpotlightToolText.StreamTwitchKeyConfigure, AiSpotlightToolParameterSets.StreamTarget, openWorld: true);
        AddSensitive(tools, AiSpotlightToolNames.StreamCustomRtmpConfigure, AiSpotlightToolText.StreamCustomRtmpConfigure, AiSpotlightToolParameterSets.StreamTarget, openWorld: true);
        AddMutation(tools, AiSpotlightToolNames.StreamTargetBind, AiSpotlightToolText.StreamTargetBind, AiSpotlightToolParameterSets.StreamTarget, openWorld: true);
        AddSensitive(tools, AiSpotlightToolNames.StreamTargetRemove, AiSpotlightToolText.StreamTargetRemove, AiSpotlightToolParameterSets.StreamTarget, destructive: true);
        AddMutation(tools, AiSpotlightToolNames.StreamDestinationArm, AiSpotlightToolText.StreamDestinationArm, AiSpotlightToolParameterSets.StreamTarget);
        AddMutation(tools, AiSpotlightToolNames.StreamDestinationUnarm, AiSpotlightToolText.StreamDestinationUnarm, AiSpotlightToolParameterSets.StreamTarget);
        AddMutation(tools, AiSpotlightToolNames.StreamGoLiveSourceSelect, AiSpotlightToolText.StreamGoLiveSourceSelect, AiSpotlightToolParameterSets.Source);
        AddMutation(tools, AiSpotlightToolNames.StreamGoLiveLayoutSet, AiSpotlightToolText.StreamGoLiveLayoutSet, AiSpotlightToolParameterSets.Layout);
        AddMutation(tools, AiSpotlightToolNames.StreamGoLiveAudioMixUpdate, AiSpotlightToolText.StreamGoLiveAudioMixUpdate, AiSpotlightToolParameterSets.Source);
        AddMutation(tools, AiSpotlightToolNames.StreamGoLiveSceneAdd, AiSpotlightToolText.StreamGoLiveSceneAdd, AiSpotlightToolParameterSets.Source);
        AddMutation(tools, AiSpotlightToolNames.StreamGoLiveSceneReorder, AiSpotlightToolText.StreamGoLiveSceneReorder, AiSpotlightToolParameterSets.Source);
        AddSensitive(tools, AiSpotlightToolNames.StreamGoLiveSceneRemove, AiSpotlightToolText.StreamGoLiveSceneRemove, AiSpotlightToolParameterSets.Source, destructive: true);
        AddSensitive(tools, AiSpotlightToolNames.StreamSourceTakeToAir, AiSpotlightToolText.StreamSourceTakeToAir, AiSpotlightToolParameterSets.Source);
        AddSensitive(tools, AiSpotlightToolNames.StreamRecordingStart, AiSpotlightToolText.StreamRecordingStart, destructive: false);
        AddSensitive(tools, AiSpotlightToolNames.StreamRecordingStop, AiSpotlightToolText.StreamRecordingStop);
        AddSensitive(tools, AiSpotlightToolNames.StreamStart, AiSpotlightToolText.StreamStart, openWorld: true);
        AddSensitive(tools, AiSpotlightToolNames.StreamStop, AiSpotlightToolText.StreamStop, openWorld: true);
    }

    private static void AddReadOnly(
        List<AiSpotlightTool> tools,
        string name,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null,
        bool openWorld = false) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            UiTextKey.SettingsNavStreaming,
            UiTextKey.SettingsStreamingSectionDescription,
            prompt,
            AiSpotlightToolScopes.Streaming,
            parameters,
            openWorld: openWorld));

    private static void AddMutation(
        List<AiSpotlightTool> tools,
        string name,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null,
        bool openWorld = false) =>
        tools.Add(AiSpotlightToolFactory.AgentTool(
            name,
            UiTextKey.SettingsNavStreaming,
            UiTextKey.SettingsStreamingSectionDescription,
            prompt,
            AiSpotlightToolScopes.Streaming,
            parameters,
            readOnly: false,
            idempotent: false,
            openWorld: openWorld));

    private static void AddSensitive(
        List<AiSpotlightTool> tools,
        string name,
        string prompt,
        IReadOnlyList<ScriptAgentAppToolParameter>? parameters = null,
        bool destructive = false,
        bool openWorld = false) =>
        tools.Add(AiSpotlightToolFactory.SensitiveMutationTool(
            name,
            UiTextKey.SettingsNavStreaming,
            UiTextKey.SettingsStreamingSectionDescription,
            prompt,
            AiSpotlightToolScopes.Streaming,
            parameters,
            destructive,
            openWorld));
}
