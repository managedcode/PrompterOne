using Azure.Identity;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using ManagedCode.Storage.CloudKit;
using ManagedCode.Storage.CloudKit.Options;
using ManagedCode.Storage.Core;
using ManagedCode.Storage.Dropbox;
using ManagedCode.Storage.Dropbox.Options;
using ManagedCode.Storage.Google;
using ManagedCode.Storage.Google.Options;
using ManagedCode.Storage.GoogleDrive;
using ManagedCode.Storage.GoogleDrive.Options;
using ManagedCode.Storage.OneDrive;
using ManagedCode.Storage.OneDrive.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace PrompterLive.Shared.Storage.Cloud;

public sealed class CloudStorageProviderFactory(
    BrowserCloudStorageStore cloudStorageStore,
    ILoggerFactory loggerFactory)
{
    private const string GoogleCloudStorageScope = "https://www.googleapis.com/auth/devstorage.full_control";
    private const string GoogleDriveApplicationName = "PrompterLive";
    private static readonly string[] GraphScopes = ["https://graph.microsoft.com/.default"];

    private readonly BrowserCloudStorageStore _cloudStorageStore = cloudStorageStore;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    public async Task<IStorage> CreateAsync(
        CloudStoragePreferences preferences,
        string providerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        return providerId switch
        {
            CloudStorageProviderIds.CloudKit => await CreateCloudKitStorageAsync(preferences.CloudKit, cancellationToken),
            CloudStorageProviderIds.Dropbox => await CreateDropboxStorageAsync(preferences.Dropbox, cancellationToken),
            CloudStorageProviderIds.GoogleCloudStorage => await CreateGoogleCloudStorageAsync(preferences.GoogleCloudStorage, cancellationToken),
            CloudStorageProviderIds.GoogleDrive => await CreateGoogleDriveStorageAsync(preferences.GoogleDrive, cancellationToken),
            CloudStorageProviderIds.OneDrive => await CreateOneDriveStorageAsync(preferences.OneDrive, cancellationToken),
            _ => throw new InvalidOperationException($"Unknown cloud storage provider '{providerId}'.")
        };
    }

    public async Task<CloudStorageOperationResult> ValidateAsync(
        CloudStoragePreferences preferences,
        string providerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var storage = await CreateAsync(preferences, providerId, cancellationToken);
            var result = await storage.CreateContainerAsync(cancellationToken);
            return result.IsSuccess
                ? CloudStorageOperationResult.Success("Connection validated.")
                : CloudStorageOperationResult.Failure(ToMessage(result.Problem, "Connection failed."));
        }
        catch (Exception exception)
        {
            return CloudStorageOperationResult.Failure(exception.Message);
        }
    }

    private async Task<IStorage> CreateDropboxStorageAsync(
        DropboxCloudStorageProfile profile,
        CancellationToken cancellationToken)
    {
        var credentials = await _cloudStorageStore.LoadDropboxCredentialsAsync(cancellationToken);
        ValidateDropboxCredentials(credentials);

        return new DropboxStorage(
            new DropboxStorageOptions
            {
                AccessToken = credentials.AccessToken,
                AppKey = credentials.AppKey,
                AppSecret = credentials.AppSecret,
                RefreshToken = credentials.RefreshToken,
                RootPath = profile.Connection.RootPath
            },
            _loggerFactory.CreateLogger<DropboxStorage>());
    }

    private async Task<IStorage> CreateOneDriveStorageAsync(
        OneDriveCloudStorageProfile profile,
        CancellationToken cancellationToken)
    {
        var credentials = await _cloudStorageStore.LoadOneDriveCredentialsAsync(cancellationToken);
        ValidateOneDriveCredentials(credentials);

        var graphClient = new GraphServiceClient(
            new ClientSecretCredential(credentials.TenantId, credentials.ClientId, credentials.ClientSecret),
            GraphScopes);

        return new OneDriveStorage(
            new OneDriveStorageOptions
            {
                DriveId = profile.DriveId,
                GraphClient = graphClient,
                RootPath = profile.Connection.RootPath
            },
            _loggerFactory.CreateLogger<OneDriveStorage>());
    }

    private async Task<IStorage> CreateGoogleDriveStorageAsync(
        GoogleDriveCloudStorageProfile profile,
        CancellationToken cancellationToken)
    {
        var credentials = await _cloudStorageStore.LoadGoogleDriveCredentialsAsync(cancellationToken);
        ValidateServiceAccountJson(credentials.ServiceAccountJson, "Google Drive");

        var driveCredential = CredentialFactory
            .FromJson<ServiceAccountCredential>(credentials.ServiceAccountJson)
            .ToGoogleCredential()
            .CreateScoped(DriveService.Scope.Drive);

        var driveService = new DriveService(new BaseClientService.Initializer
        {
            ApplicationName = GoogleDriveApplicationName,
            HttpClientInitializer = driveCredential
        });

        return new GoogleDriveStorage(
            new GoogleDriveStorageOptions
            {
                CreateContainerIfNotExists = true,
                DriveService = driveService,
                RootFolderId = profile.RootFolderId,
                SupportsAllDrives = profile.SupportsAllDrives
            },
            _loggerFactory.CreateLogger<GoogleDriveStorage>());
    }

    private async Task<IStorage> CreateGoogleCloudStorageAsync(
        GoogleCloudStorageProfile profile,
        CancellationToken cancellationToken)
    {
        var credentials = await _cloudStorageStore.LoadGoogleCloudStorageCredentialsAsync(cancellationToken);
        ValidateGoogleCloudStorage(profile, credentials);

        var googleCredential = CredentialFactory
            .FromJson<ServiceAccountCredential>(credentials.ServiceAccountJson)
            .ToGoogleCredential()
            .CreateScoped(GoogleCloudStorageScope);

        return new GCPStorage(
            new GCPStorageOptions
            {
                BucketOptions = new BucketOptions
                {
                    Bucket = profile.BucketName,
                    ProjectId = profile.ProjectId
                },
                CreateContainerIfNotExists = true,
                GoogleCredential = googleCredential
            },
            _loggerFactory.CreateLogger<GCPStorage>());
    }

    private async Task<IStorage> CreateCloudKitStorageAsync(
        CloudKitStorageProfile profile,
        CancellationToken cancellationToken)
    {
        var credentials = await _cloudStorageStore.LoadCloudKitCredentialsAsync(cancellationToken);
        ValidateCloudKit(profile, credentials);

        return new CloudKitStorage(
            new CloudKitStorageOptions
            {
                ApiToken = NullIfWhiteSpace(credentials.ApiToken),
                ContainerId = profile.ContainerId,
                Database = profile.Database,
                Environment = profile.Environment,
                RootPath = profile.Connection.RootPath,
                ServerToServerKeyId = NullIfWhiteSpace(credentials.ServerToServerKeyId),
                ServerToServerPrivateKeyPem = NullIfWhiteSpace(credentials.ServerToServerPrivateKeyPem),
                WebAuthToken = NullIfWhiteSpace(credentials.WebAuthToken)
            },
            _loggerFactory.CreateLogger<CloudKitStorage>());
    }

    private static void ValidateDropboxCredentials(DropboxCloudStorageCredentials credentials)
    {
        var hasAccessToken = !string.IsNullOrWhiteSpace(credentials.AccessToken);
        var hasRefreshFlow = !string.IsNullOrWhiteSpace(credentials.RefreshToken) &&
            !string.IsNullOrWhiteSpace(credentials.AppKey);

        if (!hasAccessToken && !hasRefreshFlow)
        {
            throw new InvalidOperationException("Dropbox requires an access token or a refresh token with app key.");
        }
    }

    private static void ValidateOneDriveCredentials(OneDriveCloudStorageCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(credentials.TenantId) ||
            string.IsNullOrWhiteSpace(credentials.ClientId) ||
            string.IsNullOrWhiteSpace(credentials.ClientSecret))
        {
            throw new InvalidOperationException("OneDrive requires tenant id, client id, and client secret.");
        }
    }

    private static void ValidateServiceAccountJson(string value, string providerName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{providerName} requires a service account JSON key.");
        }
    }

    private static void ValidateGoogleCloudStorage(
        GoogleCloudStorageProfile profile,
        GoogleCloudStorageCredentials credentials)
    {
        ValidateServiceAccountJson(credentials.ServiceAccountJson, "Google Cloud Storage");

        if (string.IsNullOrWhiteSpace(profile.ProjectId) || string.IsNullOrWhiteSpace(profile.BucketName))
        {
            throw new InvalidOperationException("Google Cloud Storage requires a project id and bucket name.");
        }
    }

    private static void ValidateCloudKit(
        CloudKitStorageProfile profile,
        CloudKitStorageCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(profile.ContainerId))
        {
            throw new InvalidOperationException("CloudKit requires a container id.");
        }

        var hasApiToken = !string.IsNullOrWhiteSpace(credentials.ApiToken);
        var hasServerCredentials = !string.IsNullOrWhiteSpace(credentials.ServerToServerKeyId) &&
            !string.IsNullOrWhiteSpace(credentials.ServerToServerPrivateKeyPem);
        var hasWebToken = !string.IsNullOrWhiteSpace(credentials.WebAuthToken);

        if (!hasApiToken && !hasServerCredentials && !hasWebToken)
        {
            throw new InvalidOperationException("CloudKit requires an API token, web auth token, or server-to-server credentials.");
        }
    }

    private static string? NullIfWhiteSpace(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static string ToMessage(ManagedCode.Communication.Problem? problem, string fallbackMessage) =>
        problem?.Detail ?? problem?.Title ?? fallbackMessage;
}
