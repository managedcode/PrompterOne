using Microsoft.AspNetCore.Components.Web;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Pages;

public partial class LearnPage : IAsyncDisposable
{
    private Task HandleLearnKeyDownAsync(KeyboardEventArgs args) =>
        HandleLearnKeyboardAsync(args);

    private async Task HandleLearnKeyboardAsync(KeyboardEventArgs args)
    {
        if (!AppHotkeys.TryResolve(AppHotkeySurface.Learn, args, out var action))
        {
            return;
        }

        switch (action)
        {
            case AppHotkeyAction.LearnBack:
                await NavigateBackToEditorAsync();
                break;
            case AppHotkeyAction.LearnPlayPause:
                await ToggleRsvpPlaybackAsync();
                break;
            case AppHotkeyAction.LearnStepBackward:
                await StepRsvpBackwardAsync();
                break;
            case AppHotkeyAction.LearnStepForward:
                await StepRsvpForwardAsync();
                break;
            case AppHotkeyAction.LearnStepBackwardLarge:
                await StepRsvpBackwardLargeAsync();
                break;
            case AppHotkeyAction.LearnStepForwardLarge:
                await StepRsvpForwardLargeAsync();
                break;
            case AppHotkeyAction.LearnSpeedUp:
                await IncreaseRsvpSpeedAsync();
                break;
            case AppHotkeyAction.LearnSpeedDown:
                await DecreaseRsvpSpeedAsync();
                break;
            case AppHotkeyAction.LearnToggleLoop:
                await ToggleLoopPlaybackAsync();
                break;
        }
    }

    public ValueTask DisposeAsync()
    {
        StopPlaybackLoop();
        return ValueTask.CompletedTask;
    }
}
