using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Tools;

internal static class AiSpotlightHotkeyToolCatalog
{
    public static void AddTo(List<AiSpotlightTool> tools)
    {
        foreach (var group in AppHotkeys.Groups)
        {
            foreach (var definition in group.Definitions)
            {
                tools.Add(HotkeyTool(group, definition));
            }
        }
    }

    private static AiSpotlightTool HotkeyTool(AppHotkeyGroup group, AppHotkeyDefinition definition)
    {
        var requiresApproval = IsApprovalRequired(definition.Action);
        return new(
            AiSpotlightToolNames.Hotkey(definition.Id),
            AiSpotlightSuggestionKind.Hotkey,
            AppHotkeyToolTextKeys.GetLabelKey(definition),
            AppHotkeyToolTextKeys.GetDescriptionKey(definition),
            AiSpotlightToolText.HotkeyPrompt(definition.Id),
            AiSpotlightToolDispatchKinds.Hotkey,
            group.Id,
            HotkeyAction: definition.Action,
            ReadOnly: false,
            Idempotent: false,
            Destructive: requiresApproval,
            OpenWorld: definition.Action is AppHotkeyAction.GoLiveToggleStream,
            RequiresApproval: requiresApproval);
    }

    private static bool IsApprovalRequired(AppHotkeyAction action) =>
        action is AppHotkeyAction.GoLiveTakeToAir
            or AppHotkeyAction.GoLiveToggleRecording
            or AppHotkeyAction.GoLiveToggleStream;

}
