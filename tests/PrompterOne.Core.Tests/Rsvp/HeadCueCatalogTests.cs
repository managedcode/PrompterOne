using ManagedCode.Tps;
using PrompterOne.Core.Models.HeadCues;

namespace PrompterOne.Core.Tests;

public sealed class HeadCueCatalogTests
{
    [Test]
    public void HeadCueCatalog_ResolvesEmotionHeadCuesFromTpsSpec()
    {
        Assert.Equal(TpsSpec.EmotionHeadCues[TpsSpec.DefaultEmotion], HeadCueCatalog.DefaultCueId);
        Assert.Equal(TpsSpec.EmotionHeadCues[TpsSpec.EmotionNames.Happy], HeadCueCatalog.ResolveForEmotion(TpsSpec.EmotionNames.Happy));
        Assert.Equal(TpsSpec.EmotionHeadCues[TpsSpec.DefaultEmotion], HeadCueCatalog.ResolveForEmotion("missing-emotion"));
    }
}
