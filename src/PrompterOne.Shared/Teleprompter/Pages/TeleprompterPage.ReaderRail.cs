using PrompterOne.Shared.Contracts;

namespace PrompterOne.Shared.Pages;

public partial class TeleprompterPage
{
    private const string ReaderFocalSliderTitle = "Move the focal reading guide";
    private const string ReaderFullscreenTitle = "Toggle browser fullscreen";
    private const string ReaderMirrorHorizontalLabel = "H";
    private const string ReaderMirrorHorizontalTitle = "Mirror the reader horizontally";
    private const string ReaderMirrorVerticalLabel = "V";
    private const string ReaderMirrorVerticalTitle = "Mirror the reader vertically";
    private const string ReaderOrientationTitle = "Rotate the reader between landscape and portrait";
    private const string ReaderRailTooltipCssClass = "rd-rail-tooltip";
    private const string ReaderRailTooltipLeftCssClass = "rd-rail-tooltip--left";
    private const string ReaderRailTooltipRightCssClass = "rd-rail-tooltip--right";
    private const string ReaderTextAlignCenterTitle = "Center text on the reading lane";
    private const string ReaderTextAlignJustifyTitle = "Stretch text across the full readable width";
    private const string ReaderTextAlignLeftTitle = "Align text to the left edge";
    private const string ReaderTextAlignRightTitle = "Align text to the right edge";
    private const string ReaderWidthSliderTitle = "Adjust the reader text width";

    private static string BuildRailTooltipCssClass(bool placeOnRightSide) =>
        placeOnRightSide
            ? $"{ReaderRailTooltipCssClass} {ReaderRailTooltipRightCssClass}"
            : $"{ReaderRailTooltipCssClass} {ReaderRailTooltipLeftCssClass}";

    private static string BuildRailTooltipDomId(string key) => UiDomIds.Teleprompter.RailTooltip(key);

    private static string BuildRailTooltipTestId(string key) => UiTestIds.Teleprompter.RailTooltip(key);
}
