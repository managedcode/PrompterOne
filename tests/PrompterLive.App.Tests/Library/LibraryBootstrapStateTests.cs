using Bunit;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Pages;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class LibraryBootstrapStateTests : Bunit.BunitContext
{
    [Fact]
    public void LibraryPage_DoesNotRenderRuntimeSeededScripts_WhenHarnessSkipsTestFixtures()
    {
        TestHarnessFactory.Create(this, seedLibraryData: false);

        var cut = Render<LibraryPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.FindByTestId(UiTestIds.Library.Page));
            Assert.NotNull(cut.FindByTestId(UiTestIds.Library.CreateScript));
            Assert.NotNull(cut.FindByTestId(UiTestIds.Library.FolderCreateTile));
            Assert.DoesNotContain(AppTestData.Scripts.DemoTitle, cut.Markup, StringComparison.Ordinal);
            Assert.DoesNotContain(AppTestData.Scripts.TedLeadershipTitle, cut.Markup, StringComparison.Ordinal);
        });
    }
}
