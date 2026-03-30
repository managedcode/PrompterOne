using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class TeleprompterKeyboardShortcutTests : BunitContext
{
    [Fact]
    public void TeleprompterPage_EscapeShortcut_NavigatesBackToEditor()
    {
        TestHarnessFactory.Create(this);
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppTestData.Routes.TeleprompterQuantum);

        var cut = Render<TeleprompterPage>();

        cut.WaitForAssertion(() =>
        {
            var page = cut.FindByTestId(UiTestIds.Teleprompter.Page);
            page.TriggerEvent("onkeydown", new KeyboardEventArgs { Key = UiKeyboardKeys.Escape });

            Assert.EndsWith(AppTestData.Routes.EditorQuantum, Services.GetRequiredService<NavigationManager>().Uri, StringComparison.Ordinal);
        });
    }
}
