using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Tools;

public enum AiSpotlightSuggestionKind
{
    Command,
    Navigation,
    Graph,
    Hotkey,
    Settings
}

public static class AiSpotlightToolDispatchKinds
{
    public const string Agent = "agent";
    public const string Hotkey = "hotkey";
    public const string Navigation = "navigation";
}

public sealed record AiSpotlightTool(
    string Name,
    AiSpotlightSuggestionKind Kind,
    UiTextKey LabelKey,
    UiTextKey DetailKey,
    string Prompt,
    string DispatchKind,
    string Scope,
    string? Route = null,
    AppHotkeyAction? HotkeyAction = null,
    bool ReadOnly = true,
    bool Idempotent = true,
    bool Destructive = false,
    bool OpenWorld = false,
    bool RequiresApproval = false,
    IReadOnlyList<ScriptAgentAppToolParameter>? Parameters = null)
{
    public ScriptAgentAppToolDescriptor ToAgentTool(Func<UiTextKey, string> text) =>
        new(
            Name,
            text(LabelKey),
            text(DetailKey),
            Scope,
            DispatchKind,
            Route,
            HotkeyAction?.ToString(),
            Prompt,
            ReadOnly,
            Idempotent,
            Destructive,
            OpenWorld,
            RequiresApproval,
            Parameters ?? []);
}
