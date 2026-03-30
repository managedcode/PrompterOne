using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Services.Streaming;

namespace PrompterLive.Shared.Pages;

public partial class GoLivePage
{
    private static string FormatRouteTarget(AudioRouteTarget routeTarget) =>
        routeTarget switch
        {
            AudioRouteTarget.Monitor => "Monitor only",
            AudioRouteTarget.Stream => "Stream only",
            _ => DefaultMicRouteLabel
        };

    private static string BuildSelectedSourceSummary(int selectedSourceCount)
    {
        return string.Concat(
            selectedSourceCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            " ",
            selectedSourceCount == 1 ? SelectedCameraSingularLabel : SelectedCameraPluralLabel);
    }

    private sealed record GoLiveDestinationState(
        bool IsEnabled,
        bool IsReady,
        string StatusLabel,
        string Summary);
}
