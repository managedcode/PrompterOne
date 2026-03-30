using Microsoft.AspNetCore.Components.Web;
using PrompterLive.Shared.Contracts;

namespace PrompterLive.Shared.Pages;

public partial class TeleprompterPage : IAsyncDisposable
{
    private Task HandleTeleprompterKeyDownAsync(KeyboardEventArgs args) =>
        HandleTeleprompterKeyboardAsync(args.Key);

    private async Task HandleTeleprompterKeyboardAsync(string? key)
    {
        switch (key)
        {
            case UiKeyboardKeys.Escape:
                await NavigateBackToEditorAsync();
                break;
            case UiKeyboardKeys.Space:
                await ToggleReaderPlaybackAsync();
                break;
            case UiKeyboardKeys.ArrowLeft:
            case UiKeyboardKeys.PageUp:
                await JumpToPreviousReaderCardAsync();
                break;
            case UiKeyboardKeys.ArrowRight:
            case UiKeyboardKeys.PageDown:
                await JumpToNextReaderCardAsync();
                break;
            case UiKeyboardKeys.CameraLower:
            case UiKeyboardKeys.CameraUpper:
                await ToggleReaderCameraAsync();
                break;
        }
    }

    public async ValueTask DisposeAsync()
    {
        StopReaderPlaybackLoop();
        await DetachReaderCameraAsync();
    }
}
