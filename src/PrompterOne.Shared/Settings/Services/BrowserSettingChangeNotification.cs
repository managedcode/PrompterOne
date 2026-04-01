namespace PrompterOne.Shared.Services;

public delegate Task BrowserSettingChangedHandler(BrowserSettingChangeNotification notification);

public interface IBrowserSettingsChangeNotifier
{
    event BrowserSettingChangedHandler? Changed;
}

public sealed record BrowserSettingChangeNotification(string Key, bool IsRemote);

internal sealed record BrowserSettingChangePayload(string Key, string ChangeKind);

internal static class BrowserSettingChangeKinds
{
    public const string Removed = "removed";
    public const string Saved = "saved";
}
