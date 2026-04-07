using PrompterOne.Shared.Components;
using PrompterOne.Shared.Localization;

namespace PrompterOne.Shared.Settings.Models;

public sealed record SettingsSidebarNavigationItem(
    PrompterOne.Shared.Pages.SettingsSection Section,
    UiIconKind Icon,
    UiTextKey LabelKey,
    string TestId);
