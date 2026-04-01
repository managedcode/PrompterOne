namespace PrompterLive.Shared.Storage;

public static class PrompterStorageDefaults
{
    public const int BrowserChunkBatchSize = 4;
    public const int BrowserChunkSizeBytes = 4 * 1024 * 1024;
    public const string LocalBrowserContainerName = "prompterlive-local";
    public const string LocalBrowserDatabaseName = "prompterlive-storage";

    public const string LibraryRootPath = "/library";
    public const string ScriptDirectoryPath = LibraryRootPath + "/scripts";
    public const string FolderDirectoryPath = LibraryRootPath + "/folders";
    public const string SettingsRootPath = "/settings";
    public const string SettingsSnapshotFilePath = SettingsRootPath + "/browser-settings.json";
}
