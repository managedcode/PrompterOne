using Microsoft.AspNetCore.Components.Web;
using PrompterOne.Shared.Components.GoLive;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Pages;

public partial class GoLivePage
{
    private Task HandleGoLiveKeyDownAsync(KeyboardEventArgs args) =>
        HandleGoLiveKeyboardAsync(args);

    private async Task HandleGoLiveKeyboardAsync(KeyboardEventArgs args)
    {
        if (!AppHotkeys.TryResolve(AppHotkeySurface.GoLive, args, out var action))
        {
            return;
        }

        switch (action)
        {
            case AppHotkeyAction.GoLiveDirectorMode:
                await SelectStudioModeAsync(GoLiveStudioMode.Director);
                break;
            case AppHotkeyAction.GoLiveStudioMode:
                await SelectStudioModeAsync(GoLiveStudioMode.Studio);
                break;
            case AppHotkeyAction.GoLiveToggleLeftRail:
                await ToggleLeftRailAsync();
                break;
            case AppHotkeyAction.GoLiveToggleRightRail:
                await ToggleRightRailAsync();
                break;
            case AppHotkeyAction.GoLiveToggleFullProgram:
                await ToggleFullProgramViewAsync();
                break;
            case AppHotkeyAction.GoLiveTakeToAir:
                if (CanSwitchProgram)
                {
                    await SwitchSelectedSourceAsync();
                }
                break;
            case AppHotkeyAction.GoLiveToggleRecording:
                await ToggleRecordingSessionAsync();
                break;
            case AppHotkeyAction.GoLiveToggleStream:
                await ToggleStreamSessionAsync();
                break;
        }
    }
}
