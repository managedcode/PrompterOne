using PrompterOne.Core.Models.Workspace;

namespace PrompterOne.Shared.Pages;

public partial class TeleprompterPage
{
    private Task SetReaderTextAlignmentAsync(ReaderTextAlignment textAlignment)
    {
        if (_readerTextAlignment == textAlignment)
        {
            return Task.CompletedTask;
        }

        _readerTextAlignment = textAlignment;
        RequestReaderAlignment(instant: true);
        return PersistCurrentReaderLayoutAsync();
    }
}
