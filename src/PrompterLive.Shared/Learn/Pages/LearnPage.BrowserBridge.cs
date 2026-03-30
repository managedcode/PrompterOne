using Microsoft.AspNetCore.Components.Web;
using PrompterLive.Shared.Contracts;

namespace PrompterLive.Shared.Pages;

public partial class LearnPage : IAsyncDisposable
{
    private Task HandleLearnKeyDownAsync(KeyboardEventArgs args) =>
        HandleLearnKeyboardAsync(args.Key);

    private async Task HandleLearnKeyboardAsync(string? key)
    {
        switch (key)
        {
            case UiKeyboardKeys.Escape:
                await NavigateBackToEditorAsync();
                break;
            case UiKeyboardKeys.Space:
                await ToggleRsvpPlaybackAsync();
                break;
            case UiKeyboardKeys.ArrowLeft:
                await StepRsvpBackwardAsync();
                break;
            case UiKeyboardKeys.ArrowRight:
                await StepRsvpForwardAsync();
                break;
            case UiKeyboardKeys.PageUp:
                await StepRsvpBackwardLargeAsync();
                break;
            case UiKeyboardKeys.PageDown:
                await StepRsvpForwardLargeAsync();
                break;
            case UiKeyboardKeys.ArrowUp:
                await IncreaseRsvpSpeedAsync();
                break;
            case UiKeyboardKeys.ArrowDown:
                await DecreaseRsvpSpeedAsync();
                break;
        }
    }

    public ValueTask DisposeAsync()
    {
        StopPlaybackLoop();
        return ValueTask.CompletedTask;
    }
}
