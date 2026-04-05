namespace PrompterOne.Web.UITests;

internal static class UiTestHostConstants
{
    public const string ApplicationMarker = "PrompterOne";
    public const string AppSettingsDevelopmentFileName = "appsettings.Development.json";
    public const string AppSettingsDevelopmentCompressedFileName = "appsettings.Development.json.gz";
    public const string AppSettingsProductionFileName = "appsettings.json";
    public const string AppSettingsProductionCompressedFileName = "appsettings.json.gz";
    public const string BlazorEnvironmentHeaderName = "Blazor-Environment";
    public const string BlankPagePath = "/_test/blank";
    public const string BrowserStorageDatabaseName = "prompterone-storage";
    public const string LoopbackBaseAddressTemplate = "http://127.0.0.1:0";
    public const int MaximumTcpPort = 65535;
    public const int MinimumDynamicPort = 1;
    public const string ProductionEnvironmentName = "Production";
    public const string RuntimeTelemetryHarnessInitializationScript =
        """
        window.__prompterOneTelemetryHarness = {
            blockVendorScripts: true,
            events: [],
            initializations: [],
            pageViews: [],
            vendorLoads: []
        };
        """;
    public static readonly string[] GrantedPermissions = ["camera", "microphone"];
    public const string ResetBrowserStorageScript =
        """
        async databaseName => {
            window.localStorage.clear();
            window.sessionStorage.clear();

            if ('caches' in window) {
                const cacheKeys = await window.caches.keys();
                await Promise.all(cacheKeys.map(cacheKey => window.caches.delete(cacheKey)));
            }

            if ('indexedDB' in window) {
                await new Promise(resolve => {
                    const request = window.indexedDB.deleteDatabase(databaseName);
                    request.onsuccess = () => resolve();
                    request.onerror = () => resolve();
                    request.onblocked = () => resolve();
                });
            }
        }
        """;
}
