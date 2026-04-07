using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Pages;
using PrompterOne.Shared.Tests;
using PrompterOne.Testing.Editor;

namespace PrompterOne.Web.Tests;

[NotInParallel]
public sealed class EditorSplitFeedbackInteractionTests : BunitContext
{
    private static readonly TimeSpan AutosaveAssertionTimeout = TimeSpan.FromSeconds(5);
    private readonly AppHarness _harness;

    public EditorSplitFeedbackInteractionTests()
    {
        _harness = TestHarnessFactory.Create(this);
    }

    [Test]
    public void EditorPage_SplitFeedbackStaysVisibleAcrossRedundantSourceChangeEvents()
    {
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo(AppTestData.Routes.EditorDemo);
        var cut = Render<EditorPage>();

        cut.WaitForAssertion(() =>
        {
            var source = cut.FindByTestId(UiTestIds.Editor.SourceInput);
            Assert.Contains(AppTestData.Editor.BodyHeading, source.GetAttribute("value"));
        });

        cut.FindByTestId(UiTestIds.Editor.SourceInput).Input(EditorSplitFeedbackTestData.SplitSource);
        cut.FindByTestId(UiTestIds.Editor.SplitSegment).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitFeedbackTitle,
                cut.FindByTestId(UiTestIds.Editor.SplitResultTitle).TextContent.Trim());
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitActionLabel,
                cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
        });

        cut.FindByTestId(UiTestIds.Editor.SourceInput).Input(EditorSplitFeedbackTestData.SplitSource);

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitActionLabel,
                cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
        }, AutosaveAssertionTimeout);

        cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.EndsWith(AppRoutes.Library, navigationManager.Uri, StringComparison.Ordinal);
        });
    }

    [Test]
    public void EditorPage_SplitFeedbackStaysVisibleAfterAutosaveRefresh()
    {
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo(AppTestData.Routes.EditorDemo);
        var cut = Render<EditorPage>();

        cut.WaitForAssertion(() =>
        {
            var source = cut.FindByTestId(UiTestIds.Editor.SourceInput);
            Assert.Contains(AppTestData.Editor.BodyHeading, source.GetAttribute("value"));
        });

        cut.FindByTestId(UiTestIds.Editor.SourceInput).Input(EditorSplitFeedbackTestData.SplitSource);
        cut.FindByTestId(UiTestIds.Editor.SplitSegment).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitActionLabel,
                cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
        });

        cut.WaitForAssertion(() =>
        {
            var persistedDocument = _harness.Repository
                .GetAsync(AppTestData.Scripts.DemoId)
                .GetAwaiter()
                .GetResult();

            Assert.NotNull(persistedDocument);
            Assert.Contains(
                EditorSplitFeedbackTestData.SplitSource,
                persistedDocument!.Text,
                StringComparison.Ordinal);
        }, AutosaveAssertionTimeout);

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitActionLabel,
                cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
        }, AutosaveAssertionTimeout);

        cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.EndsWith(AppRoutes.Library, navigationManager.Uri, StringComparison.Ordinal);
        });
    }

    [Test]
    public void EditorPage_SplitFeedbackStaysVisibleAfterSourceEdits()
    {
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo(AppTestData.Routes.EditorDemo);
        var cut = Render<EditorPage>();

        cut.WaitForAssertion(() =>
        {
            var source = cut.FindByTestId(UiTestIds.Editor.SourceInput);
            Assert.Contains(AppTestData.Editor.BodyHeading, source.GetAttribute("value"));
        });

        cut.FindByTestId(UiTestIds.Editor.SourceInput).Input(EditorSplitFeedbackTestData.SplitSource);
        cut.FindByTestId(UiTestIds.Editor.SplitSegment).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitActionLabel,
                cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
        }, AutosaveAssertionTimeout);

        cut.FindByTestId(UiTestIds.Editor.SourceInput).Input(EditorSplitFeedbackTestData.EditedSplitSource);

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitActionLabel,
                cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
        });

        cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.EndsWith(AppRoutes.Library, navigationManager.Uri, StringComparison.Ordinal);
        });
    }

    [Test]
    public void EditorPage_SplitFeedbackStaysVisibleAcrossUntitledDraftAutosaveNavigation()
    {
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo(AppRoutes.Editor);
        var cut = Render<EditorPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.FindByTestId(UiTestIds.Editor.SourceInput));
        });

        cut.FindByTestId(UiTestIds.Editor.SourceInput).Input(EditorSplitFeedbackTestData.SplitSource);
        cut.FindByTestId(UiTestIds.Editor.SplitSegment).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitFeedbackDestination,
                cut.FindByTestId(UiTestIds.Editor.SplitResultLibrary).TextContent.Trim());
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitActionLabel,
                cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
        }, AutosaveAssertionTimeout);

        cut.WaitForAssertion(() =>
        {
            var uri = new Uri(navigationManager.Uri);
            Assert.Equal(AppRoutes.Editor, uri.AbsolutePath);
            Assert.Contains(
                $"{AppRoutes.ScriptIdQueryKey}=",
                uri.Query,
                StringComparison.Ordinal);
        }, AutosaveAssertionTimeout);

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitFeedbackDestination,
                cut.FindByTestId(UiTestIds.Editor.SplitResultLibrary).TextContent.Trim());
            Assert.Equal(
                EditorSplitFeedbackTestData.SplitActionLabel,
                cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
        }, AutosaveAssertionTimeout);
    }
}
