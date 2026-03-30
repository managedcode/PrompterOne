using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PrompterLive.Shared.Contracts;
using PrompterLive.Shared.Layout;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class MainLayoutActionTests : BunitContext
{
    [Theory]
    [InlineData(AppRoutes.Library)]
    [InlineData(AppRoutes.Settings)]
    public void MainLayout_RendersGoLiveAction_OnEveryNonGoLiveScreen(string route)
    {
        _ = TestHarnessFactory.Create(this);
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(route);

        var cut = Render<MainLayout>(parameters => parameters
            .Add(layout => layout.Body, (RenderFragment)(builder => builder.AddMarkupContent(0, "<div>Body</div>"))));

        cut.WaitForAssertion(() => Assert.NotNull(cut.FindByTestId(UiTestIds.Header.GoLive)));
    }
}
