using PrompterLive.Core.Localization;

namespace PrompterLive.Core.Tests;

public sealed class AppCultureCatalogTests
{
    [Theory]
    [InlineData("fr-FR", AppCultureCatalog.FrenchCultureName)]
    [InlineData("uk-UA", AppCultureCatalog.UkrainianCultureName)]
    [InlineData("pt-BR", AppCultureCatalog.PortugueseCultureName)]
    [InlineData("ru-RU", AppCultureCatalog.EnglishCultureName)]
    [InlineData("de-DE", AppCultureCatalog.EnglishCultureName)]
    [InlineData("", AppCultureCatalog.EnglishCultureName)]
    public void ResolveSupportedCulture_NormalizesBrowserCultureNames(string requestedCulture, string expectedCulture)
    {
        var actualCulture = AppCultureCatalog.ResolveSupportedCulture(requestedCulture);

        Assert.Equal(expectedCulture, actualCulture);
    }

    [Fact]
    public void ResolvePreferredCulture_UsesFirstSupportedCulture_AndBlocksRussian()
    {
        var actualCulture = AppCultureCatalog.ResolvePreferredCulture(["ru-RU", "es-ES", "it-IT"]);

        Assert.Equal(AppCultureCatalog.EnglishCultureName, actualCulture);
    }
}
