using ManagedCode.Storage.CloudKit.Options;
using PrompterLive.Core.Models.Documents;
using PrompterLive.Core.Models.Library;
using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Shared.Settings.Models;

namespace PrompterLive.Shared.Storage.Cloud;

public sealed class CloudStorageConnectionState
{
    public string AccountLabel { get; set; } = string.Empty;

    public bool IsConnected { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset? LastValidatedAt { get; set; }

    public string RootPath { get; set; } = string.Empty;
}

public sealed class DropboxCloudStorageProfile
{
    public CloudStorageConnectionState Connection { get; set; } = new()
    {
        RootPath = "/apps/prompterlive"
    };
}

public sealed class OneDriveCloudStorageProfile
{
    public CloudStorageConnectionState Connection { get; set; } = new()
    {
        RootPath = "PrompterLive"
    };

    public string DriveId { get; set; } = "me";
}

public sealed class GoogleDriveCloudStorageProfile
{
    public CloudStorageConnectionState Connection { get; set; } = new()
    {
        RootPath = "prompterlive"
    };

    public string RootFolderId { get; set; } = "root";

    public bool SupportsAllDrives { get; set; } = true;
}

public sealed class GoogleCloudStorageProfile
{
    public string BucketName { get; set; } = string.Empty;

    public CloudStorageConnectionState Connection { get; set; } = new()
    {
        RootPath = "prompterlive"
    };

    public string ProjectId { get; set; } = string.Empty;
}

public sealed class CloudKitStorageProfile
{
    public string ContainerId { get; set; } = string.Empty;

    public CloudStorageConnectionState Connection { get; set; } = new()
    {
        RootPath = "prompterlive"
    };

    public CloudKitDatabase Database { get; set; } = CloudKitDatabase.Public;

    public CloudKitEnvironment Environment { get; set; } = CloudKitEnvironment.Development;
}

public sealed class CloudStoragePreferences
{
    public bool AutoSyncOnSave { get; set; } = true;

    public CloudKitStorageProfile CloudKit { get; set; } = new();

    public DropboxCloudStorageProfile Dropbox { get; set; } = new();

    public GoogleCloudStorageProfile GoogleCloudStorage { get; set; } = new();

    public GoogleDriveCloudStorageProfile GoogleDrive { get; set; } = new();

    public OneDriveCloudStorageProfile OneDrive { get; set; } = new();

    public string PrimaryProviderId { get; set; } = CloudStorageProviderIds.OneDrive;

    public bool SyncOnStartup { get; set; } = true;

    public static CloudStoragePreferences CreateDefault() => new();

    public CloudStoragePreferences Normalize()
    {
        CloudKit ??= new CloudKitStorageProfile();
        Dropbox ??= new DropboxCloudStorageProfile();
        GoogleCloudStorage ??= new GoogleCloudStorageProfile();
        GoogleDrive ??= new GoogleDriveCloudStorageProfile();
        OneDrive ??= new OneDriveCloudStorageProfile();
        PrimaryProviderId = string.IsNullOrWhiteSpace(PrimaryProviderId) ? CloudStorageProviderIds.OneDrive : PrimaryProviderId;
        return this;
    }
}

public sealed class DropboxCloudStorageCredentials
{
    public string AccessToken { get; set; } = string.Empty;

    public string AppKey { get; set; } = string.Empty;

    public string AppSecret { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class OneDriveCloudStorageCredentials
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;
}

public sealed class GoogleDriveCloudStorageCredentials
{
    public string ServiceAccountJson { get; set; } = string.Empty;
}

public sealed class GoogleCloudStorageCredentials
{
    public string ServiceAccountJson { get; set; } = string.Empty;
}

public sealed class CloudKitStorageCredentials
{
    public string ApiToken { get; set; } = string.Empty;

    public string ServerToServerKeyId { get; set; } = string.Empty;

    public string ServerToServerPrivateKeyPem { get; set; } = string.Empty;

    public string WebAuthToken { get; set; } = string.Empty;
}

public sealed class CloudStorageSettingsBundle
{
    public LearnSettings LearnSettings { get; set; } = new();

    public ReaderSettings ReaderSettings { get; set; } = new();

    public MediaSceneState SceneState { get; set; } = MediaSceneState.Empty;

    public SettingsPagePreferences SettingsPagePreferences { get; set; } = SettingsPagePreferences.Default;

    public StudioSettings StudioSettings { get; set; } = StudioSettings.Default;
}

public sealed class CloudStorageBackupEnvelope
{
    public const string CurrentSchemaVersion = "1";

    public DateTimeOffset ExportedAt { get; set; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<StoredLibraryFolder> Folders { get; set; } = Array.Empty<StoredLibraryFolder>();

    public string SchemaVersion { get; set; } = CurrentSchemaVersion;

    public IReadOnlyList<StoredScriptDocument> Scripts { get; set; } = Array.Empty<StoredScriptDocument>();

    public CloudStorageSettingsBundle Settings { get; set; } = new();
}

public sealed record CloudStorageOperationResult(bool IsSuccess, string Message)
{
    public static CloudStorageOperationResult Failure(string message) => new(false, message);

    public static CloudStorageOperationResult Success(string message) => new(true, message);
}
