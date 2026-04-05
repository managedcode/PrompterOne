using Microsoft.AspNetCore.Components.Web;

namespace PrompterOne.Shared.Contracts;

public enum AppHotkeySurface
{
    Editor,
    Learn,
    Teleprompter,
    GoLive
}

public enum AppHotkeyAction
{
    EditorUndo,
    EditorRedo,
    EditorSelectAll,
    LearnBack,
    LearnPlayPause,
    LearnSpeedDown,
    LearnSpeedUp,
    LearnStepBackward,
    LearnStepBackwardLarge,
    LearnStepForward,
    LearnStepForwardLarge,
    LearnToggleLoop,
    TeleprompterBack,
    TeleprompterPlayPause,
    TeleprompterPreviousBlock,
    TeleprompterNextBlock,
    TeleprompterMirrorHorizontal,
    TeleprompterMirrorVertical,
    TeleprompterOrientation,
    TeleprompterFullscreen,
    TeleprompterAlignmentLeft,
    TeleprompterAlignmentCenter,
    TeleprompterAlignmentRight,
    TeleprompterAlignmentJustify,
    TeleprompterCamera,
    GoLiveDirectorMode,
    GoLiveStudioMode,
    GoLiveToggleLeftRail,
    GoLiveToggleRightRail,
    GoLiveToggleFullProgram,
    GoLiveTakeToAir,
    GoLiveToggleRecording,
    GoLiveToggleStream
}

public readonly record struct AppKeyboardShortcut(
    string DisplayText,
    string Key,
    bool Shift = false,
    bool Alt = false,
    bool Ctrl = false)
{
    public bool Matches(KeyboardEventArgs args) =>
        string.Equals(UiKeyboardKeys.Normalize(args.Key), Key, StringComparison.Ordinal)
        && args.ShiftKey == Shift
        && args.AltKey == Alt
        && args.CtrlKey == Ctrl;
}

public sealed record AppHotkeyDefinition(
    string Id,
    AppHotkeyAction Action,
    string ActionLabel,
    string Description,
    IReadOnlyList<AppKeyboardShortcut> Shortcuts);

public sealed record AppHotkeyGroup(
    AppHotkeySurface Surface,
    string Id,
    string Title,
    string Subtitle,
    IReadOnlyList<AppHotkeyDefinition> Definitions);

public static class AppHotkeys
{
    public static IReadOnlyList<AppHotkeyGroup> Groups { get; } =
    [
        new(
            AppHotkeySurface.Editor,
            "editor",
            "Editor",
            "Core authoring shortcuts",
            [
                new("editor-undo", AppHotkeyAction.EditorUndo, "Undo", "Revert the last editor change.", [new("Ctrl/Cmd+Z", UiKeyboardKeys.ZLower, Ctrl: true)]),
                new("editor-redo", AppHotkeyAction.EditorRedo, "Redo", "Re-apply the last reverted editor change.", [new("Ctrl/Cmd+Shift+Z", UiKeyboardKeys.ZLower, Shift: true, Ctrl: true)]),
                new("editor-select-all", AppHotkeyAction.EditorSelectAll, "Select all", "Select the whole document in the source editor.", [new("Ctrl/Cmd+A", UiKeyboardKeys.ALower, Ctrl: true)])
            ]),
        new(
            AppHotkeySurface.Learn,
            "learn",
            "Learn",
            "RSVP rehearsal controls",
            [
                new("learn-back", AppHotkeyAction.LearnBack, "Back to editor", "Leave Learn and return to the current script.", [new("Esc", UiKeyboardKeys.Escape)]),
                new("learn-play-pause", AppHotkeyAction.LearnPlayPause, "Play or pause", "Start or pause RSVP playback.", [new("Space", UiKeyboardKeys.Space)]),
                new("learn-speed-down", AppHotkeyAction.LearnSpeedDown, "Slow down", "Reduce RSVP speed.", [new("Down", UiKeyboardKeys.ArrowDown)]),
                new("learn-speed-up", AppHotkeyAction.LearnSpeedUp, "Speed up", "Increase RSVP speed.", [new("Up", UiKeyboardKeys.ArrowUp)]),
                new("learn-step-backward", AppHotkeyAction.LearnStepBackward, "Previous word", "Step back one word.", [new("Left", UiKeyboardKeys.ArrowLeft)]),
                new("learn-step-backward-large", AppHotkeyAction.LearnStepBackwardLarge, "Previous phrase jump", "Step back five words.", [new("PageUp", UiKeyboardKeys.PageUp)]),
                new("learn-step-forward", AppHotkeyAction.LearnStepForward, "Next word", "Step forward one word.", [new("Right", UiKeyboardKeys.ArrowRight)]),
                new("learn-step-forward-large", AppHotkeyAction.LearnStepForwardLarge, "Next phrase jump", "Step forward five words.", [new("PageDown", UiKeyboardKeys.PageDown)]),
                new("learn-toggle-loop", AppHotkeyAction.LearnToggleLoop, "Loop playback", "Toggle RSVP loop mode.", [new("L", UiKeyboardKeys.LLower)])
            ]),
        new(
            AppHotkeySurface.Teleprompter,
            "teleprompter",
            "Teleprompter",
            "Reader playback and display controls",
            [
                new("teleprompter-back", AppHotkeyAction.TeleprompterBack, "Back to editor", "Exit fullscreen when needed, then return to the script.", [new("Esc", UiKeyboardKeys.Escape)]),
                new("teleprompter-play-pause", AppHotkeyAction.TeleprompterPlayPause, "Play or pause", "Start or pause teleprompter playback.", [new("Space", UiKeyboardKeys.Space)]),
                new("teleprompter-previous-block", AppHotkeyAction.TeleprompterPreviousBlock, "Previous block", "Jump to the previous reader block.", [new("Left", UiKeyboardKeys.ArrowLeft), new("PageUp", UiKeyboardKeys.PageUp)]),
                new("teleprompter-next-block", AppHotkeyAction.TeleprompterNextBlock, "Next block", "Jump to the next reader block.", [new("Right", UiKeyboardKeys.ArrowRight), new("PageDown", UiKeyboardKeys.PageDown)]),
                new("teleprompter-mirror-horizontal", AppHotkeyAction.TeleprompterMirrorHorizontal, "Mirror horizontally", "Flip the reader horizontally.", [new("H", UiKeyboardKeys.HLower)]),
                new("teleprompter-mirror-vertical", AppHotkeyAction.TeleprompterMirrorVertical, "Mirror vertically", "Flip the reader vertically.", [new("V", UiKeyboardKeys.VLower)]),
                new("teleprompter-orientation", AppHotkeyAction.TeleprompterOrientation, "Rotate reader", "Toggle landscape and portrait reader orientation.", [new("O", UiKeyboardKeys.OLower)]),
                new("teleprompter-fullscreen", AppHotkeyAction.TeleprompterFullscreen, "Toggle fullscreen", "Enter or leave browser fullscreen.", [new("F", UiKeyboardKeys.FLower)]),
                new("teleprompter-alignment-left", AppHotkeyAction.TeleprompterAlignmentLeft, "Align left", "Use the left-aligned reader lane.", [new("1", UiKeyboardKeys.Digit1)]),
                new("teleprompter-alignment-center", AppHotkeyAction.TeleprompterAlignmentCenter, "Align center", "Use the centered reader lane.", [new("2", UiKeyboardKeys.Digit2)]),
                new("teleprompter-alignment-right", AppHotkeyAction.TeleprompterAlignmentRight, "Align right", "Use the right-aligned reader lane.", [new("3", UiKeyboardKeys.Digit3)]),
                new("teleprompter-alignment-justify", AppHotkeyAction.TeleprompterAlignmentJustify, "Align justify", "Stretch text across the full readable width.", [new("4", UiKeyboardKeys.Digit4)]),
                new("teleprompter-camera", AppHotkeyAction.TeleprompterCamera, "Toggle camera", "Show or hide the reader camera layer.", [new("C", UiKeyboardKeys.CLower)])
            ]),
        new(
            AppHotkeySurface.GoLive,
            "go-live",
            "Go Live",
            "Studio and session controls",
            [
                new("go-live-director-mode", AppHotkeyAction.GoLiveDirectorMode, "Director mode", "Switch the studio to Director mode.", [new("1", UiKeyboardKeys.Digit1)]),
                new("go-live-studio-mode", AppHotkeyAction.GoLiveStudioMode, "Studio mode", "Switch the studio to Studio mode.", [new("2", UiKeyboardKeys.Digit2)]),
                new("go-live-left-rail", AppHotkeyAction.GoLiveToggleLeftRail, "Toggle source rail", "Show or hide the left source rail.", [new("[", UiKeyboardKeys.BracketLeft)]),
                new("go-live-right-rail", AppHotkeyAction.GoLiveToggleRightRail, "Toggle output rail", "Show or hide the right preview rail.", [new("]", UiKeyboardKeys.BracketRight)]),
                new("go-live-full-program", AppHotkeyAction.GoLiveToggleFullProgram, "Toggle full program", "Expand or collapse the full-program monitor layout.", [new("F", UiKeyboardKeys.FLower)]),
                new("go-live-take-to-air", AppHotkeyAction.GoLiveTakeToAir, "Take to air", "Switch the selected source to program.", [new("Enter", UiKeyboardKeys.Enter)]),
                new("go-live-recording", AppHotkeyAction.GoLiveToggleRecording, "Start or stop recording", "Toggle local recording for the active program.", [new("R", UiKeyboardKeys.RLower)]),
                new("go-live-stream", AppHotkeyAction.GoLiveToggleStream, "Start or stop streaming", "Toggle the live stream session.", [new("S", UiKeyboardKeys.SLower)])
            ])
    ];

    public static IReadOnlyList<AppHotkeyDefinition> GetDefinitions(AppHotkeySurface surface) =>
        Groups.First(group => group.Surface == surface).Definitions;

    public static bool TryResolve(AppHotkeySurface surface, KeyboardEventArgs args, out AppHotkeyAction action)
    {
        foreach (var definition in GetDefinitions(surface))
        {
            if (definition.Shortcuts.Any(shortcut => shortcut.Matches(args)))
            {
                action = definition.Action;
                return true;
            }
        }

        action = default;
        return false;
    }
}
