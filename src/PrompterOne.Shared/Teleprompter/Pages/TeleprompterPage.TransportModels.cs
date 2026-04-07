using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Pages;

public partial class TeleprompterPage
{
    private IReadOnlyList<TeleprompterEdgeSegmentViewModel> BuildReaderEdgeSegments() =>
        _cards
            .Select((card, index) => new TeleprompterEdgeSegmentViewModel(BuildEdgeSegmentStyle(card, index)))
            .ToArray();

    private IReadOnlyList<TeleprompterProgressSegmentViewModel> BuildReaderProgressSegments() =>
        _cards
            .Select((card, index) => new TeleprompterProgressSegmentViewModel(
                BuildReaderProgressSegmentFillStyle(index),
                UiTestIds.Teleprompter.ProgressSegmentFill(index),
                BuildReaderProgressSegmentStyle(card)))
            .ToArray();
}
