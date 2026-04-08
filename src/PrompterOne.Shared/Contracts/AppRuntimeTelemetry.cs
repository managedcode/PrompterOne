namespace PrompterOne.Shared.Contracts;

public static class AppRuntimeTelemetry
{
    public static class Harness
    {
        public const string EventsCollection = "events";
        public const string GlobalName = "__prompterOneTelemetryHarness";
        public const string InitializationsCollection = "initializations";
        public const string PageViewsCollection = "pageViews";
        public const string RuntimeAllowVendorLoadsProperty = "telemetryAllowVendorLoads";
        public const string RuntimeGlobalName = "__prompterOneRuntime";
        public const string RuntimeHarnessEnabledProperty = "telemetryHarnessEnabled";
        public const string RuntimeWasmDebugEnabledProperty = "wasmDebugEnabled";
        public const string VendorLoadsCollection = "vendorLoads";
    }

    public static class Events
    {
        public const string CreateScript = "create_script";
        public const string OpenGoLive = "open_go_live";
        public const string OpenLearn = "open_learn";
        public const string OpenRead = "open_read";
        public const string PageView = "page_view";
    }

    public static class Pages
    {
        public const string Editor = "editor";
        public const string GoLive = "go_live";
        public const string Learn = "learn";
        public const string Library = "library";
        public const string Settings = "settings";
        public const string Teleprompter = "teleprompter";
    }

    public static class Parameters
    {
        public const string PagePath = "page_path";
        public const string PageTitle = "page_title";
        public const string ScreenName = "screen_name";
        public const string ScriptLoaded = "script_loaded";
        public const string SourceScreen = "source_screen";
        public const string TargetScreen = "target_screen";
    }

    public static class Titles
    {
        public const string Editor = "Editor";
        public const string GoLive = "Go Live";
        public const string Learn = "Learn";
        public const string Library = "Library";
        public const string Settings = "Settings";
        public const string Teleprompter = "Teleprompter";
    }

    public static string GetDefaultTitle(AppShellScreen screen) => screen switch
    {
        AppShellScreen.Editor => Titles.Editor,
        AppShellScreen.GoLive => Titles.GoLive,
        AppShellScreen.Learn => Titles.Learn,
        AppShellScreen.Library => Titles.Library,
        AppShellScreen.Settings => Titles.Settings,
        AppShellScreen.Teleprompter => Titles.Teleprompter,
        _ => Titles.Library
    };

    public static string GetPageName(AppShellScreen screen) => screen switch
    {
        AppShellScreen.Editor => Pages.Editor,
        AppShellScreen.GoLive => Pages.GoLive,
        AppShellScreen.Learn => Pages.Learn,
        AppShellScreen.Library => Pages.Library,
        AppShellScreen.Settings => Pages.Settings,
        AppShellScreen.Teleprompter => Pages.Teleprompter,
        _ => Pages.Library
    };

    public static string GetRoutePath(AppShellScreen screen) => screen switch
    {
        AppShellScreen.Editor => AppRoutes.Editor,
        AppShellScreen.GoLive => AppRoutes.GoLive,
        AppShellScreen.Learn => AppRoutes.Learn,
        AppShellScreen.Library => AppRoutes.Library,
        AppShellScreen.Settings => AppRoutes.Settings,
        AppShellScreen.Teleprompter => AppRoutes.Teleprompter,
        _ => AppRoutes.Library
    };
}
