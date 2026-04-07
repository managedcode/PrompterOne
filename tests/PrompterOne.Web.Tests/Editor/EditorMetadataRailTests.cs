using Bunit;
using PrompterOne.Core.Models.Editor;
using PrompterOne.Shared.Components.Editor;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Tests;

namespace PrompterOne.Web.Tests;

public sealed class EditorMetadataRailTests : BunitContext
{
    private const string SplitActionLabel = "Open in Library";
    private const string SplitFeedbackBadge = "From ##";
    private const string SplitFeedbackDestination = "New scripts were saved to the library.";
    private const string SplitFeedbackDraftNote = "The current draft stayed open.";
    private const string SplitFeedbackSummary = "2 new scripts created.";
    private const string SplitFeedbackTitle = "Split complete";
    private const string SplitHint = "Create separate scripts from headings in the current draft.";
    private const string SplitSection = "Split Into New Scripts";
    private const string SplitTopLevelLabel = "New scripts from # headings";
    private const string SplitSegmentLabel = "New scripts from ## headings";
    private const string CreatedEpisodeOne = "Episode 1 - How to Think About Systems";
    private const string CreatedEpisodeTwo = "Episode 2 - How Systems Talk to Each Other";
    private const string CreatedEpisodeThree = "Episode 3 - Event Sourcing and CQRS";

    public EditorMetadataRailTests()
    {
        TestHarnessFactory.Create(this);
    }

    [Test]
    public void EditorMetadataRail_SplitActionsInvokeExpectedModes_AndRenderSplitFeedback()
    {
        var requestedModes = new List<TpsDocumentSplitMode>();
        var openLibraryRequests = 0;
        var cut = Render<EditorMetadataRail>(parameters => parameters
            .Add(component => component.Title, "Product Launch")
            .Add(component => component.Profile, "Actor")
            .Add(component => component.Status, new EditorStatusViewModel(2, 4, "Actor", 140, 0, 0, 256, "1:30", "1.0"))
            .Add(component => component.LocalHistory, [])
            .Add(component => component.SplitFeedback, BuildSplitFeedback())
            .Add(component => component.OpenLibraryRequested, () => openLibraryRequests++)
            .Add(component => component.SplitRequested, mode => requestedModes.Add(mode)));

        Assert.Contains(SplitSection, cut.Markup, StringComparison.Ordinal);
        Assert.Equal(SplitHint, cut.Find($".ed-meta-action-note").TextContent.Trim());
        Assert.Equal(SplitTopLevelLabel, cut.FindByTestId(UiTestIds.Editor.SplitTopLevel).TextContent.Trim());
        Assert.Equal(SplitSegmentLabel, cut.FindByTestId(UiTestIds.Editor.SplitSegment).TextContent.Trim());

        cut.FindByTestId(UiTestIds.Editor.SplitTopLevel).Click();
        cut.FindByTestId(UiTestIds.Editor.SplitSegment).Click();
        cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).Click();

        Assert.Equal(
            [TpsDocumentSplitMode.TopLevelHeading, TpsDocumentSplitMode.SegmentHeading],
            requestedModes);
        Assert.Equal(1, openLibraryRequests);
        Assert.Equal(SplitFeedbackTitle, cut.FindByTestId(UiTestIds.Editor.SplitResultTitle).TextContent.Trim());
        Assert.Equal(SplitFeedbackSummary, cut.FindByTestId(UiTestIds.Editor.SplitResultSummary).TextContent.Trim());
        Assert.Equal(SplitFeedbackBadge, cut.FindByTestId(UiTestIds.Editor.SplitResultBadge).TextContent.Trim());
        Assert.Equal(SplitFeedbackDestination, cut.FindByTestId(UiTestIds.Editor.SplitResultLibrary).TextContent.Trim());
        Assert.Equal(SplitFeedbackDraftNote, cut.FindByTestId(UiTestIds.Editor.SplitResultCurrentDraft).TextContent.Trim());
        Assert.Equal(CreatedEpisodeOne, cut.FindByTestId(UiTestIds.Editor.SplitResultItem(0)).TextContent.Trim().Replace("01", string.Empty).Trim());
        Assert.Equal(CreatedEpisodeTwo, cut.FindByTestId(UiTestIds.Editor.SplitResultItem(1)).TextContent.Trim().Replace("02", string.Empty).Trim());
        Assert.Equal(CreatedEpisodeThree, cut.FindByTestId(UiTestIds.Editor.SplitResultItem(2)).TextContent.Trim().Replace("03", string.Empty).Trim());
        Assert.Equal("+2 more in Library", cut.FindByTestId(UiTestIds.Editor.SplitResultMore).TextContent.Trim());
        Assert.Equal(SplitActionLabel, cut.FindByTestId(UiTestIds.Editor.SplitResultOpenLibrary).TextContent.Trim());
    }

    private static EditorSplitFeedbackViewModel BuildSplitFeedback() =>
        new(
            Title: SplitFeedbackTitle,
            Summary: SplitFeedbackSummary,
            HeadingBadge: SplitFeedbackBadge,
            DestinationNote: SplitFeedbackDestination,
            DraftNote: SplitFeedbackDraftNote,
            OpenLibraryLabel: SplitActionLabel,
            CreatedTitles:
            [
                CreatedEpisodeOne,
                CreatedEpisodeTwo,
                CreatedEpisodeThree
            ],
            AdditionalCount: 2);
}
