using PrompterOne.Core.Models.Media;

namespace PrompterOne.Shared.Pages;

public partial class GoLivePage
{
    private static string FormatRouteTarget(AudioRouteTarget routeTarget) =>
        routeTarget switch
        {
            AudioRouteTarget.Monitor => "Monitor only",
            AudioRouteTarget.Stream => "Stream only",
            _ => DefaultMicRouteLabel
        };

    private sealed record GoLiveDestinationState(
        bool IsEnabled,
        bool IsReady,
        string StatusLabel,
        string Summary);
}
