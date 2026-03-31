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
        await AlignReaderCardTextAsync(_activeReaderCardIndex, Math.Max(_activeReaderWordIndex, 0), neutralizeCard: false, rerender: true);
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

    private Task PrepareReaderCardAlignmentAsync(int cardIndex, int wordOrdinal) =>
        AlignReaderCardTextAsync(cardIndex, wordOrdinal, neutralizeCard: true, rerender: false);

    private async Task AlignReaderCardTextAsync(int cardIndex, int wordOrdinal, bool neutralizeCard, bool rerender)
    {
        if (!TryGetAlignmentWordId(cardIndex, wordOrdinal, out var wordId))
        {
            return;
        }

        var offsetPixels = await ReaderInterop.MeasureClusterOffsetAsync(
            UiDomIds.Teleprompter.Stage,
            UiDomIds.Teleprompter.CardText(cardIndex),
            wordId,
            _readerFocalPointPercent,
            neutralizeCard);

        if (!offsetPixels.HasValue)
        {
            return;
        }

        var nextStyle = BuildReaderCardTextStyleValue(offsetPixels.Value);
        if (_readerCardTextStyles.TryGetValue(cardIndex, out var currentStyle) &&
            string.Equals(currentStyle, nextStyle, StringComparison.Ordinal))
        {
            return;
        }

        _readerCardTextStyles[cardIndex] = nextStyle;
        if (rerender)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private bool TryGetAlignmentWordId(int cardIndex, int wordOrdinal, out string wordId)
    {
        wordId = string.Empty;
        if (!TryGetAlignmentWordPosition(cardIndex, wordOrdinal, out var position))
        {
            return false;
        }

        wordId = UiDomIds.Teleprompter.CardWord(position.CardIndex, position.ChunkIndex, position.WordIndex);
        return true;
    }

    private bool TryGetAlignmentWordPosition(int cardIndex, int wordOrdinal, out (int CardIndex, int ChunkIndex, int WordIndex) position)
    {
        position = default;
        if (cardIndex < 0 || cardIndex >= _cards.Count)
        {
            return false;
        }

        var remainingWords = Math.Max(wordOrdinal, 0);
        var chunks = _cards[cardIndex].Chunks;

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
                    position = (cardIndex, chunkIndex, wordIndex);
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
