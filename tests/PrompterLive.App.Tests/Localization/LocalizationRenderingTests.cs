using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using PrompterLive.Core.Localization;
using PrompterLive.Shared.Components.Diagnostics;
using PrompterLive.Shared.Components.Library;
using PrompterLive.Shared.Localization;
using PrompterLive.Shared.Tests;

namespace PrompterLive.App.Tests;

public sealed class LocalizationRenderingTests : BunitContext
{
    public LocalizationRenderingTests()
    {
        TestHarnessFactory.Create(this);
    }

    [Fact]
    public void LibrarySidebar_RendersUkrainianLabels_WhenCurrentCultureIsUkrainian()
    {
        using var _ = new CultureScope(AppCultureCatalog.UkrainianCultureName);

        var cut = Render<LibrarySidebar>(parameters => parameters
            .Add(component => component.Folders, [])
            .Add(component => component.AllScriptCount, 5)
            .Add(component => component.IsAllSelected, true)
            .Add(component => component.OnSelectFolder, EventCallback.Factory.Create<string>(this, _ => Task.CompletedTask))
            .Add(component => component.OnStartCreateFolder, EventCallback.Factory.Create(this, () => Task.CompletedTask)));

        Assert.Contains(UiTextCatalog.Get(UiTextKey.LibraryAllScripts), cut.Markup);
        Assert.Contains(UiTextCatalog.Get(UiTextKey.LibraryFavorites), cut.Markup);
        Assert.Contains(UiTextCatalog.Get(UiTextKey.LibrarySettings), cut.Markup);
    }

    [Fact]
    public void LibraryFolderCreateModal_RendersFrenchLabels_WhenCurrentCultureIsFrench()
    {
        using var _ = new CultureScope(AppCultureCatalog.FrenchCultureName);

        var cut = Render<LibraryFolderCreateModal>(parameters => parameters
            .Add(component => component.FolderOptions, [])
            .Add(component => component.FolderDraftName, string.Empty)
            .Add(component => component.FolderDraftParentId, LibrarySelectionKeys.Root)
            .Add(component => component.OnCancelCreateFolder, EventCallback.Factory.Create(this, () => Task.CompletedTask))
            .Add(component => component.OnSubmitCreateFolder, EventCallback.Factory.Create(this, () => Task.CompletedTask))
            .Add(component => component.OnFolderDraftNameChanged, EventCallback.Factory.Create<string>(this, _ => Task.CompletedTask))
            .Add(component => component.OnFolderDraftParentChanged, EventCallback.Factory.Create<string>(this, _ => Task.CompletedTask)));

        Assert.Contains(UiTextCatalog.Get(UiTextKey.LibraryCreateFolderTitle), cut.Markup);
        Assert.Contains(UiTextCatalog.Get(UiTextKey.CommonCreate), cut.Markup);
        Assert.Contains(UiTextCatalog.Get(UiTextKey.CommonCancel), cut.Markup);
    }

    [Fact]
    public void DiagnosticsBanner_RendersItalianDismissLabel_WhenCurrentCultureIsItalian()
    {
        using var _ = new CultureScope(AppCultureCatalog.ItalianCultureName);
        var diagnostics = Services.GetRequiredService<PrompterLive.Shared.Services.Diagnostics.UiDiagnosticsService>();
        diagnostics.ReportRecoverable("diagnostics", "Localized diagnostics", "detail");

        var cut = Render<DiagnosticsBanner>();

        Assert.Contains(UiTextCatalog.Get(UiTextKey.DiagnosticsDismiss), cut.Markup);
    }

    [Fact]
    public void LoggingErrorBoundary_RendersLocalizedFatalActions_WhenCurrentCultureIsFrench()
    {
        using var _ = new CultureScope(AppCultureCatalog.FrenchCultureName);

        var cut = Render<LoggingErrorBoundary>(parameters => parameters
            .AddChildContent<ThrowingLocalizationDiagnosticsComponent>());

        Assert.Contains(UiTextCatalog.Get(UiTextKey.DiagnosticsRetry), cut.Markup);
        Assert.Contains(UiTextCatalog.Get(UiTextKey.DiagnosticsLibrary), cut.Markup);
        Assert.Contains(UiTextCatalog.Get(UiTextKey.DiagnosticsFatalTitle), cut.Markup);
    }

    private sealed class ThrowingLocalizationDiagnosticsComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            throw new InvalidOperationException("Forced localized boundary failure.");
        }
    }
}
