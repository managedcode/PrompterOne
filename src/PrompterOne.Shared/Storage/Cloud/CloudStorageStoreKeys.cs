namespace PrompterOne.Shared.Storage.Cloud;

public static class CloudStorageStoreKeys
{
    public const string BackupDirectory = "prompterone";
    public const string BackupFileName = "scripts-settings.json";
    public const string BackupPath = BackupDirectory + "/" + BackupFileName;
    public const string CloudKitCredentials = "prompterone.cloud-storage.credentials.cloudkit";
    public const string DropboxCredentials = "prompterone.cloud-storage.credentials.dropbox";
    public const string GoogleCloudStorageCredentials = "prompterone.cloud-storage.credentials.google-cloud-storage";
    public const string GoogleDriveCredentials = "prompterone.cloud-storage.credentials.google-drive";
    public const string JsonMimeType = "application/json";
    public const string OneDriveCredentials = "prompterone.cloud-storage.credentials.onedrive";
    public const string Preferences = "prompterone.cloud-storage.preferences";
}
