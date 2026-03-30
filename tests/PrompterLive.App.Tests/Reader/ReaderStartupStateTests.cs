using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class ReaderStartupStateTests : Bunit.BunitContext
{
    [Fact]
    public void LearnPage_DoesNotRenderPlaceholderCopyBeforeScriptLoadCompletes()
    {
        TestHarnessFactory.Create(this, jsInvocationDelay: ReaderStartupTestDelays.JavascriptLoadDelay);
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppTestData.Routes.LearnQuantum);

        var cut = Render<LearnPage>();

        Assert.DoesNotContain(ReaderStartupPlaceholderTexts.LearnFocusWord, cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(ReaderStartupPlaceholderTexts.LearnNextPhrase, cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void TeleprompterPage_DoesNotRenderPlaceholderCopyBeforeScriptLoadCompletes()
    {
        TestHarnessFactory.Create(this, jsInvocationDelay: ReaderStartupTestDelays.JavascriptLoadDelay);
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppTestData.Routes.TeleprompterQuantum);

        var cut = Render<TeleprompterPage>();

        Assert.DoesNotContain(ReaderStartupPlaceholderTexts.TeleprompterWord, cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(ReaderStartupPlaceholderTexts.TeleprompterTitle, cut.Markup, StringComparison.Ordinal);
    }

    private static class ReaderStartupPlaceholderTexts
    {
        public const string LearnFocusWord = "transformative";
        public const string LearnNextPhrase = "Today, we're not just launching a product";
        public const string TeleprompterTitle = "Product Launch";
        public const string TeleprompterWord = "Ready";
    }

    private static class ReaderStartupTestDelays
    {
        public static TimeSpan JavascriptLoadDelay => TimeSpan.FromMilliseconds(300);
    }
}
