using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class LearnKeyboardShortcutTests : BunitContext
{
    [Fact]
    public void LearnPage_EscapeShortcut_NavigatesBackToEditor()
    {
        TestHarnessFactory.Create(this);
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(AppTestData.Routes.LearnQuantum);

        var cut = Render<LearnPage>();

        cut.WaitForAssertion(() =>
        {
            var page = cut.FindByTestId(UiTestIds.Learn.Page);
            page.TriggerEvent("onkeydown", new KeyboardEventArgs { Key = UiKeyboardKeys.Escape });

            Assert.EndsWith(AppTestData.Routes.EditorQuantum, Services.GetRequiredService<NavigationManager>().Uri, StringComparison.Ordinal);
        });
    }
}
