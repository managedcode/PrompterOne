using Bunit;
using PrompterOne.Shared.Components.Editor;
using PrompterOne.Shared.Contracts;
using PrompterOne.Shared.Tests;

namespace PrompterOne.Web.Tests;

public sealed class EditorStructureSidebarTests : BunitContext
{
    private const string FocusedAccentColor = "#16A34A";
    private const string WarmEmotion = "Warm";
    private const string EpisodeTitle = "Episode 1";
    private const string BlockTitle = "Opening";

    public EditorStructureSidebarTests()
    {
        TestHarnessFactory.Create(this);
    }

    [Test]
    public void EditorStructureSidebar_NavigationButtonsInvokeExpectedTargets()
    {
        var navigationTargets = new List<EditorNavigationTarget>();
        var cut = Render<EditorStructureSidebar>(parameters => parameters
            .Add(component => component.ActiveSegmentIndex, 0)
            .Add(component => component.ActiveBlockIndex, 0)
            .Add(component => component.Segments, BuildSegments())
            .Add(component => component.OnNavigate, target => navigationTargets.Add(target)));

        cut.FindByTestId(UiTestIds.Editor.SegmentNavigation(0)).Click();
        cut.FindByTestId(UiTestIds.Editor.BlockNavigation(0, 0)).Click();

        Assert.Equal(
            [
                new EditorNavigationTarget(0, null, 0, 20),
                new EditorNavigationTarget(0, 0, 0, 20)
            ],
            navigationTargets);
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
