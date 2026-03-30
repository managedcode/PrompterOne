namespace PrompterLive.Shared.Pages;

public partial class SettingsPage
{
    private const string ActiveCssClass = "active";
    private const string DisplayNoneStyle = "display:none";
    private const string OnCssClass = "on";

    private SettingsSection _activeSection = SettingsSection.Cloud;
    private string _selectedAiProviderId = SettingsAiProviderIds.ClaudeApi;
    private bool _ambientGradientMotionEnabled = true;
    private bool _autoSaveEnabled = true;
    private bool _backupCopiesEnabled = true;
    private bool _showHeaderChromeEnabled = true;

    private string AmbientGradientMotionToggleCssClass => BuildToggleCssClass(_ambientGradientMotionEnabled);

    private string FileAutoSaveToggleCssClass => BuildToggleCssClass(_autoSaveEnabled);

    private string FileBackupCopiesToggleCssClass => BuildToggleCssClass(_backupCopiesEnabled);

    private string ShowHeaderChromeToggleCssClass => BuildToggleCssClass(_showHeaderChromeEnabled);

    private string GetAiProviderCssClass(string providerId) =>
        string.Equals(providerId, _selectedAiProviderId, StringComparison.Ordinal)
            ? $"{SetAiProviderCssClass} {ActiveCssClass}"
            : SetAiProviderCssClass;

    private string GetNavItemCssClass(SettingsSection section) =>
        section == _activeSection ? $"{SetNavItemCssClass} {ActiveCssClass}" : SetNavItemCssClass;

    private string GetSectionDisplayStyle(SettingsSection section) =>
        section == _activeSection ? string.Empty : DisplayNoneStyle;

    private void SelectAiProvider(string providerId) => _selectedAiProviderId = providerId;

    private void ShowSection(SettingsSection section) => _activeSection = section;

    private void ToggleAmbientGradientMotion() => _ambientGradientMotionEnabled = !_ambientGradientMotionEnabled;

    private void ToggleAutoSave() => _autoSaveEnabled = !_autoSaveEnabled;

    private void ToggleBackupCopies() => _backupCopiesEnabled = !_backupCopiesEnabled;

    private void ToggleShowHeaderChrome() => _showHeaderChromeEnabled = !_showHeaderChromeEnabled;

    private static string BuildToggleCssClass(bool isOn) =>
        isOn ? $"{SetToggleCssClass} {OnCssClass}" : SetToggleCssClass;
}
