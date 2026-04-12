using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

internal static class AppHotkeyToolTextKeys
{
    public static UiTextKey GetDescriptionKey(AppHotkeyDefinition definition) => definition.Id switch
    {
        AppHotkeyIds.Definitions.GlobalOpenAssistant => UiTextKey.SettingsHotkeyGlobalOpenAssistantDescription,
        AppHotkeyIds.Definitions.EditorUndo => UiTextKey.SettingsHotkeyEditorUndoDescription,
        AppHotkeyIds.Definitions.EditorRedo => UiTextKey.SettingsHotkeyEditorRedoDescription,
        AppHotkeyIds.Definitions.EditorSelectAll => UiTextKey.SettingsHotkeyEditorSelectAllDescription,
        AppHotkeyIds.Definitions.LearnBack => UiTextKey.SettingsHotkeyLearnBackDescription,
        AppHotkeyIds.Definitions.LearnPlayPause => UiTextKey.SettingsHotkeyLearnPlayPauseDescription,
        AppHotkeyIds.Definitions.LearnSpeedDown => UiTextKey.SettingsHotkeyLearnSpeedDownDescription,
        AppHotkeyIds.Definitions.LearnSpeedUp => UiTextKey.SettingsHotkeyLearnSpeedUpDescription,
        AppHotkeyIds.Definitions.LearnStepBackward => UiTextKey.SettingsHotkeyLearnStepBackwardDescription,
        AppHotkeyIds.Definitions.LearnStepBackwardLarge => UiTextKey.SettingsHotkeyLearnStepBackwardLargeDescription,
        AppHotkeyIds.Definitions.LearnStepForward => UiTextKey.SettingsHotkeyLearnStepForwardDescription,
        AppHotkeyIds.Definitions.LearnStepForwardLarge => UiTextKey.SettingsHotkeyLearnStepForwardLargeDescription,
        AppHotkeyIds.Definitions.LearnToggleLoop => UiTextKey.SettingsHotkeyLearnLoopDescription,
        AppHotkeyIds.Definitions.TeleprompterBack => UiTextKey.SettingsHotkeyTeleprompterBackDescription,
        AppHotkeyIds.Definitions.TeleprompterPlayPause => UiTextKey.SettingsHotkeyTeleprompterPlayPauseDescription,
        AppHotkeyIds.Definitions.TeleprompterPreviousBlock => UiTextKey.SettingsHotkeyTeleprompterPreviousBlockDescription,
        AppHotkeyIds.Definitions.TeleprompterNextBlock => UiTextKey.SettingsHotkeyTeleprompterNextBlockDescription,
        AppHotkeyIds.Definitions.TeleprompterMirrorHorizontal => UiTextKey.SettingsHotkeyTeleprompterMirrorHorizontalDescription,
        AppHotkeyIds.Definitions.TeleprompterMirrorVertical => UiTextKey.SettingsHotkeyTeleprompterMirrorVerticalDescription,
        AppHotkeyIds.Definitions.TeleprompterOrientation => UiTextKey.SettingsHotkeyTeleprompterOrientationDescription,
        AppHotkeyIds.Definitions.TeleprompterFullscreen => UiTextKey.SettingsHotkeyTeleprompterFullscreenDescription,
        AppHotkeyIds.Definitions.TeleprompterAlignmentLeft => UiTextKey.SettingsHotkeyTeleprompterAlignLeftDescription,
        AppHotkeyIds.Definitions.TeleprompterAlignmentCenter => UiTextKey.SettingsHotkeyTeleprompterAlignCenterDescription,
        AppHotkeyIds.Definitions.TeleprompterAlignmentRight => UiTextKey.SettingsHotkeyTeleprompterAlignRightDescription,
        AppHotkeyIds.Definitions.TeleprompterAlignmentJustify => UiTextKey.SettingsHotkeyTeleprompterAlignJustifyDescription,
        AppHotkeyIds.Definitions.TeleprompterCamera => UiTextKey.SettingsHotkeyTeleprompterCameraDescription,
        AppHotkeyIds.Definitions.GoLiveDirectorMode => UiTextKey.SettingsHotkeyGoLiveDirectorModeDescription,
        AppHotkeyIds.Definitions.GoLiveStudioMode => UiTextKey.SettingsHotkeyGoLiveStudioModeDescription,
        AppHotkeyIds.Definitions.GoLiveToggleLeftRail => UiTextKey.SettingsHotkeyGoLiveLeftRailDescription,
        AppHotkeyIds.Definitions.GoLiveToggleRightRail => UiTextKey.SettingsHotkeyGoLiveRightRailDescription,
        AppHotkeyIds.Definitions.GoLiveToggleFullProgram => UiTextKey.SettingsHotkeyGoLiveFullProgramDescription,
        AppHotkeyIds.Definitions.GoLiveTakeToAir => UiTextKey.SettingsHotkeyGoLiveTakeToAirDescription,
        AppHotkeyIds.Definitions.GoLiveToggleRecording => UiTextKey.SettingsHotkeyGoLiveRecordingDescription,
        AppHotkeyIds.Definitions.GoLiveToggleStream => UiTextKey.SettingsHotkeyGoLiveStreamDescription,
        _ => UiTextKey.AiSpotlightIdleHint
    };

    public static UiTextKey GetLabelKey(AppHotkeyDefinition definition) => definition.Id switch
    {
        AppHotkeyIds.Definitions.GlobalOpenAssistant => UiTextKey.SettingsHotkeyGlobalOpenAssistantLabel,
        AppHotkeyIds.Definitions.EditorUndo => UiTextKey.SettingsHotkeyEditorUndoLabel,
        AppHotkeyIds.Definitions.EditorRedo => UiTextKey.SettingsHotkeyEditorRedoLabel,
        AppHotkeyIds.Definitions.EditorSelectAll => UiTextKey.SettingsHotkeyEditorSelectAllLabel,
        AppHotkeyIds.Definitions.LearnBack => UiTextKey.SettingsHotkeyLearnBackLabel,
        AppHotkeyIds.Definitions.LearnPlayPause => UiTextKey.SettingsHotkeyLearnPlayPauseLabel,
        AppHotkeyIds.Definitions.LearnSpeedDown => UiTextKey.SettingsHotkeyLearnSpeedDownLabel,
        AppHotkeyIds.Definitions.LearnSpeedUp => UiTextKey.SettingsHotkeyLearnSpeedUpLabel,
        AppHotkeyIds.Definitions.LearnStepBackward => UiTextKey.SettingsHotkeyLearnStepBackwardLabel,
        AppHotkeyIds.Definitions.LearnStepBackwardLarge => UiTextKey.SettingsHotkeyLearnStepBackwardLargeLabel,
        AppHotkeyIds.Definitions.LearnStepForward => UiTextKey.SettingsHotkeyLearnStepForwardLabel,
        AppHotkeyIds.Definitions.LearnStepForwardLarge => UiTextKey.SettingsHotkeyLearnStepForwardLargeLabel,
        AppHotkeyIds.Definitions.LearnToggleLoop => UiTextKey.SettingsHotkeyLearnLoopLabel,
        AppHotkeyIds.Definitions.TeleprompterBack => UiTextKey.SettingsHotkeyTeleprompterBackLabel,
        AppHotkeyIds.Definitions.TeleprompterPlayPause => UiTextKey.SettingsHotkeyTeleprompterPlayPauseLabel,
        AppHotkeyIds.Definitions.TeleprompterPreviousBlock => UiTextKey.SettingsHotkeyTeleprompterPreviousBlockLabel,
        AppHotkeyIds.Definitions.TeleprompterNextBlock => UiTextKey.SettingsHotkeyTeleprompterNextBlockLabel,
        AppHotkeyIds.Definitions.TeleprompterMirrorHorizontal => UiTextKey.SettingsHotkeyTeleprompterMirrorHorizontalLabel,
        AppHotkeyIds.Definitions.TeleprompterMirrorVertical => UiTextKey.SettingsHotkeyTeleprompterMirrorVerticalLabel,
        AppHotkeyIds.Definitions.TeleprompterOrientation => UiTextKey.SettingsHotkeyTeleprompterOrientationLabel,
        AppHotkeyIds.Definitions.TeleprompterFullscreen => UiTextKey.SettingsHotkeyTeleprompterFullscreenLabel,
        AppHotkeyIds.Definitions.TeleprompterAlignmentLeft => UiTextKey.SettingsHotkeyTeleprompterAlignLeftLabel,
        AppHotkeyIds.Definitions.TeleprompterAlignmentCenter => UiTextKey.SettingsHotkeyTeleprompterAlignCenterLabel,
        AppHotkeyIds.Definitions.TeleprompterAlignmentRight => UiTextKey.SettingsHotkeyTeleprompterAlignRightLabel,
        AppHotkeyIds.Definitions.TeleprompterAlignmentJustify => UiTextKey.SettingsHotkeyTeleprompterAlignJustifyLabel,
        AppHotkeyIds.Definitions.TeleprompterCamera => UiTextKey.SettingsHotkeyTeleprompterCameraLabel,
        AppHotkeyIds.Definitions.GoLiveDirectorMode => UiTextKey.SettingsHotkeyGoLiveDirectorModeLabel,
        AppHotkeyIds.Definitions.GoLiveStudioMode => UiTextKey.SettingsHotkeyGoLiveStudioModeLabel,
        AppHotkeyIds.Definitions.GoLiveToggleLeftRail => UiTextKey.SettingsHotkeyGoLiveLeftRailLabel,
        AppHotkeyIds.Definitions.GoLiveToggleRightRail => UiTextKey.SettingsHotkeyGoLiveRightRailLabel,
        AppHotkeyIds.Definitions.GoLiveToggleFullProgram => UiTextKey.SettingsHotkeyGoLiveFullProgramLabel,
        AppHotkeyIds.Definitions.GoLiveTakeToAir => UiTextKey.SettingsHotkeyGoLiveTakeToAirLabel,
        AppHotkeyIds.Definitions.GoLiveToggleRecording => UiTextKey.SettingsHotkeyGoLiveRecordingLabel,
        AppHotkeyIds.Definitions.GoLiveToggleStream => UiTextKey.SettingsHotkeyGoLiveStreamLabel,
        _ => UiTextKey.HeaderAiSpotlight
    };
}
