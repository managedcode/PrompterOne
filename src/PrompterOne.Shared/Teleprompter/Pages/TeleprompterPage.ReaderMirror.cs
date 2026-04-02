namespace PrompterOne.Shared.Pages;

public partial class TeleprompterPage
{
    private async Task ToggleReaderMirrorHorizontalAsync()
    {
        _isReaderMirrorHorizontal = !_isReaderMirrorHorizontal;
        await PersistCurrentReaderLayoutAsync();
    }

    private async Task ToggleReaderMirrorVerticalAsync()
    {
        _isReaderMirrorVertical = !_isReaderMirrorVertical;
        await PersistCurrentReaderLayoutAsync();
    }
}
