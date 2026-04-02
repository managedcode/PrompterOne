namespace PrompterOne.App.UITests;

internal static partial class BrowserTestConstants
{
    public static class LibraryResponsive
    {
        public const string ScenarioName = "library-responsive-layout";
        public const string InitialStep = "01-initial";
        public const int ViewportEdgeTolerancePx = 2;

        public const int IphoneSmallWidth = 375;
        public const int IphoneSmallHeight = 667;
        public const int IphoneMediumWidth = 390;
        public const int IphoneMediumHeight = 844;
        public const int IphoneLargeWidth = 430;
        public const int IphoneLargeHeight = 932;
        public const int IpadMiniWidth = 768;
        public const int IpadMiniHeight = 1024;
        public const int IpadWidth = 820;
        public const int IpadHeight = 1180;
        public const int IpadProWidth = 1024;
        public const int IpadProHeight = 1366;
        public const int LaptopCompactWidth = 1280;
        public const int LaptopCompactHeight = 800;
        public const int LaptopStandardWidth = 1366;
        public const int LaptopStandardHeight = 768;
        public const int LaptopLargeWidth = 1440;
        public const int LaptopLargeHeight = 900;
        public const int LaptopWideWidth = 1728;
        public const int LaptopWideHeight = 1117;

        public static IReadOnlyList<(string Name, int Width, int Height)> Viewports { get; } =
        [
            ("iphone-small", IphoneSmallWidth, IphoneSmallHeight),
            ("iphone-medium", IphoneMediumWidth, IphoneMediumHeight),
            ("iphone-large", IphoneLargeWidth, IphoneLargeHeight),
            ("ipad-mini", IpadMiniWidth, IpadMiniHeight),
            ("ipad", IpadWidth, IpadHeight),
            ("ipad-pro", IpadProWidth, IpadProHeight),
            ("laptop-compact", LaptopCompactWidth, LaptopCompactHeight),
            ("laptop-standard", LaptopStandardWidth, LaptopStandardHeight),
            ("laptop-large", LaptopLargeWidth, LaptopLargeHeight),
            ("laptop-wide", LaptopWideWidth, LaptopWideHeight)
        ];
    }
}
