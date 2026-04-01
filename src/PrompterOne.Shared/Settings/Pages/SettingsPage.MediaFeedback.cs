using PrompterOne.Core.Models.Media;

namespace PrompterOne.Shared.Pages;

public partial class SettingsPage
{
    private bool IsCameraSectionActive => _activeSection == SettingsSection.Cameras;

    private bool IsMicrophoneSectionActive => _activeSection == SettingsSection.Mics;

    private MediaDeviceInfo? SelectedMicrophone => ResolveSelectedMicrophone();
}
