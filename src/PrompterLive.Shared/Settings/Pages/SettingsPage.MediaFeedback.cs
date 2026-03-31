using PrompterLive.Core.Models.Media;

namespace PrompterLive.Shared.Pages;

public partial class SettingsPage
{
    private bool IsCameraSectionActive => _activeSection == SettingsSection.Cameras;

    private bool IsMicrophoneSectionActive => _activeSection == SettingsSection.Mics;

    private MediaDeviceInfo? SelectedMicrophone => ResolveSelectedMicrophone();
}
