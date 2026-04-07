using ManagedCode.Tps;

namespace PrompterOne.Core.Services.Rsvp;

/// <summary>
/// Analyzes words for emotional context and exposes TPS emotion keys only.
/// </summary>
public sealed class RsvpEmotionAnalyzer
{
    private static readonly HashSet<string> SupportedEmotions = new(TpsSpec.Emotions, StringComparer.OrdinalIgnoreCase);

    private string _currentEmotionKey = TpsSpec.DefaultEmotion;

    public string CurrentEmotionKey => _currentEmotionKey;

    public string? AnalyzeWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return null;
        }

        var upperWord = word.ToUpperInvariant();

        if (ContainsAny(upperWord, "HAPPY", "JOY", "SMILE", "WONDERFUL", "BEAUTIFUL", "AMAZING", "FANTASTIC"))
        {
            return TpsSpec.EmotionNames.Happy;
        }

        if (ContainsAny(upperWord, "EXCITED", "THRILLED", "INCREDIBLE", "WOW", "AWESOME"))
        {
            return TpsSpec.EmotionNames.Excited;
        }

        if (ContainsAny(upperWord, "CALM", "PEACEFUL", "RELAX", "TRANQUIL", "SERENE", "GENTLE", "SERENITY", "BREATH", "FLOW", "WASH"))
        {
            return TpsSpec.EmotionNames.Calm;
        }

        if (ContainsAny(upperWord, "SAD", "MELANCHOLY", "RAIN", "LOST", "MEMORIES", "TEARS"))
        {
            return TpsSpec.EmotionNames.Sad;
        }

        if (ContainsAny(upperWord, "FEAR", "DANGER", "ANXIETY", "WORRY", "SCARED", "INTENSE"))
        {
            return TpsSpec.EmotionNames.Concerned;
        }

        if (ContainsAny(upperWord, "ANGRY", "FURIOUS", "RAGE", "MAD", "FRUSTRATED"))
        {
            return TpsSpec.EmotionNames.Urgent;
        }

        if (ContainsAny(upperWord, "ENERGETIC", "ENERGY", "TRANSFORM", "EXCITING", "URGENT"))
        {
            return TpsSpec.EmotionNames.Energetic;
        }

        if (ContainsAny(upperWord, "FOCUS", "CONCENTRATE", "ANALYZE"))
        {
            return TpsSpec.EmotionNames.Focused;
        }

        if (ContainsAny(upperWord, "PROFESSIONAL", "DATA", "STATISTICAL", "PERFORMANCE"))
        {
            return TpsSpec.EmotionNames.Professional;
        }

        return null;
    }

    public bool UpdateEmotionForWord(string word)
    {
        var nextEmotion = AnalyzeWord(word);
        if (nextEmotion is null)
        {
            if (_currentEmotionKey == TpsSpec.DefaultEmotion)
            {
                return false;
            }

            _currentEmotionKey = TpsSpec.DefaultEmotion;
            return true;
        }

        if (nextEmotion == _currentEmotionKey)
        {
            return false;
        }

        _currentEmotionKey = nextEmotion;
        return true;
    }

    public void SetEmotion(string emotionKey)
    {
        if (SupportedEmotions.Contains(emotionKey))
        {
            _currentEmotionKey = emotionKey;
        }
    }

    public void ResetToDefault()
    {
        _currentEmotionKey = TpsSpec.DefaultEmotion;
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
