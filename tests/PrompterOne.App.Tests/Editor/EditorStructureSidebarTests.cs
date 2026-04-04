using Bunit;
using PrompterOne.Core.Models.Editor;
using PrompterOne.Shared.Components.Editor;
using PrompterOne.Shared.Contracts;

namespace PrompterOne.App.Tests;

public sealed class EditorStructureSidebarTests : BunitContext
{
    private const string FocusedAccentColor = "#16A34A";
    private const string SplitStatusMessage = "2 scripts created from ## headings.";
    private const string WarmEmotion = "Warm";
    private const string EpisodeTitle = "Episode 1";
    private const string BlockTitle = "Opening";

    [Fact]
    public void EditorStructureSidebar_SplitButtonsInvokeExpectedModes_AndRenderStatus()
    {
        var requestedModes = new List<TpsDocumentSplitMode>();
        var cut = Render<EditorStructureSidebar>(parameters => parameters
            .Add(component => component.ActiveSegmentIndex, 0)
            .Add(component => component.ActiveBlockIndex, 0)
            .Add(component => component.Segments, BuildSegments())
            .Add(component => component.StatusMessage, SplitStatusMessage)
            .Add(component => component.OnSplitRequested, mode => requestedModes.Add(mode)));

        cut.FindByTestId(UiTestIds.Editor.SplitTopLevel).Click();
        cut.FindByTestId(UiTestIds.Editor.SplitSegment).Click();

        Assert.Equal(
            [TpsDocumentSplitMode.TopLevelHeading, TpsDocumentSplitMode.SegmentHeading],
            requestedModes);
        Assert.Equal(SplitStatusMessage, cut.FindByTestId(UiTestIds.Editor.SplitStatus).TextContent.Trim());
    }

    private static IReadOnlyList<EditorOutlineSegmentViewModel> BuildSegments() =>
    [
        new(
            Index: 0,
            Name: EpisodeTitle,
            EmotionKey: "focused",
            EmotionLabel: WarmEmotion,
            AccentColor: FocusedAccentColor,
            TargetWpm: 140,
            DurationLabel: "0:30",
            StartIndex: 0,
            EndIndex: 20,
            Blocks:
            [
                new EditorOutlineBlockViewModel(
                    Index: 0,
                    Name: BlockTitle,
                    EmotionLabel: WarmEmotion,
                    TargetWpm: 140,
                    StartIndex: 0,
                    EndIndex: 20)
            ])
    ];
}
