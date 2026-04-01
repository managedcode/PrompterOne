namespace PrompterLive.Shared.Storage.Cloud;

public static class CloudStorageStoreKeys
{
    public const string BackupDirectory = "prompterlive";
    public const string BackupFileName = "scripts-settings.json";
    public const string BackupPath = BackupDirectory + "/" + BackupFileName;
    public const string CloudKitCredentials = "prompterlive.cloud-storage.credentials.cloudkit";
    public const string DropboxCredentials = "prompterlive.cloud-storage.credentials.dropbox";
    public const string GoogleCloudStorageCredentials = "prompterlive.cloud-storage.credentials.google-cloud-storage";
    public const string GoogleDriveCredentials = "prompterlive.cloud-storage.credentials.google-drive";
    public const string JsonMimeType = "application/json";
    public const string OneDriveCredentials = "prompterlive.cloud-storage.credentials.onedrive";
    public const string Preferences = "prompterlive.cloud-storage.preferences";
}
