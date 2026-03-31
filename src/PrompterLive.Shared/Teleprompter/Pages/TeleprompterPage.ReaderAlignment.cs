using System.Globalization;
using PrompterLive.Shared.Contracts;

namespace PrompterLive.Shared.Pages;

public partial class TeleprompterPage
{
    private const double ReaderAlignmentChangeThresholdPixels = 0.5d;

    private readonly Dictionary<int, string> _readerCardTextStyles = [];
    private bool _pendingReaderAlignment;

    private async Task AlignActiveReaderTextAsync()
    {
        if (!_pendingReaderAlignment || _cards.Count == 0 || _activeReaderCardIndex >= _cards.Count)
        {
            return;
        }

        _pendingReaderAlignment = false;

        if (!TryGetAlignmentWordId(out var wordId))
        {
            return;
        }

        var offsetPixels = await ReaderInterop.MeasureClusterOffsetAsync(
            UiDomIds.Teleprompter.Stage,
            UiDomIds.Teleprompter.CardText(_activeReaderCardIndex),
            wordId,
            _readerFocalPointPercent);

        if (!offsetPixels.HasValue)
        {
            return;
        }

        var nextStyle = BuildReaderCardTextStyleValue(offsetPixels.Value);
        if (_readerCardTextStyles.TryGetValue(_activeReaderCardIndex, out var currentStyle) &&
            string.Equals(currentStyle, nextStyle, StringComparison.Ordinal))
        {
            return;
        }

        _readerCardTextStyles[_activeReaderCardIndex] = nextStyle;
        await InvokeAsync(StateHasChanged);
    }

    private string BuildReaderCardTextStyle(int cardIndex) =>
        _readerCardTextStyles.TryGetValue(cardIndex, out var style)
            ? style
            : string.Empty;

    private void RequestReaderAlignment() => _pendingReaderAlignment = true;

    private void ResetReaderAlignmentState()
    {
        _readerCardTextStyles.Clear();
        _pendingReaderAlignment = true;
    }

    private bool TryGetAlignmentWordId(out string wordId)
    {
        wordId = string.Empty;
        if (!TryGetAlignmentWordPosition(out var position))
        {
            return false;
        }

        wordId = UiDomIds.Teleprompter.CardWord(position.CardIndex, position.ChunkIndex, position.WordIndex);
        return true;
    }

    private bool TryGetAlignmentWordPosition(out (int CardIndex, int ChunkIndex, int WordIndex) position)
    {
        position = default;
        if (_activeReaderCardIndex >= _cards.Count)
        {
            return false;
        }

        var remainingWords = Math.Max(_activeReaderWordIndex, 0);
        var chunks = _cards[_activeReaderCardIndex].Chunks;

        for (var chunkIndex = 0; chunkIndex < chunks.Count; chunkIndex++)
        {
            if (chunks[chunkIndex] is not ReaderGroupViewModel group)
            {
                continue;
            }

            for (var wordIndex = 0; wordIndex < group.Words.Count; wordIndex++)
            {
                if (remainingWords == 0)
                {
                    position = (_activeReaderCardIndex, chunkIndex, wordIndex);
                    return true;
                }

                remainingWords--;
            }
        }

        return false;
    }

    private static string BuildReaderCardTextStyleValue(double offsetPixels)
    {
        var roundedOffset = Math.Round(offsetPixels, 2, MidpointRounding.AwayFromZero);
        return Math.Abs(roundedOffset) <= ReaderAlignmentChangeThresholdPixels
            ? string.Empty
            : $"transform:translateY({roundedOffset.ToString("0.##", CultureInfo.InvariantCulture)}px);";
    }
}
