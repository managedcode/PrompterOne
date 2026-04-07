using ManagedCode.Tps;

namespace PrompterOne.Core.Models.HeadCues;

public static class HeadCueCatalog
{
    public static string DefaultCueId => TpsSpec.EmotionHeadCues[TpsSpec.DefaultEmotion];

    public static string ResolveForEmotion(string? emotion)
    {
        if (string.IsNullOrWhiteSpace(emotion))
        {
            return DefaultCueId;
        }

        return TpsSpec.EmotionHeadCues.TryGetValue(emotion, out var cueId)
            ? cueId
            : DefaultCueId;
    }
}
