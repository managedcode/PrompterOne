using PrompterOne.Shared.Storage;

namespace PrompterOne.Shared.Services;

internal static class BrowserLibraryStoragePaths
{
    private const string JsonFileExtension = ".json";

    public static string FolderDirectoryPath => PrompterStorageDefaults.FolderDirectoryPath;

    public static string ScriptDirectoryPath => PrompterStorageDefaults.ScriptDirectoryPath;

    public static string FolderFilePath(string folderId) => BuildJsonFilePath(FolderDirectoryPath, folderId);

    public static string ScriptFilePath(string scriptId) => BuildJsonFilePath(ScriptDirectoryPath, scriptId);

    private static string BuildJsonFilePath(string directoryPath, string itemId) =>
        string.Concat(directoryPath, "/", itemId, JsonFileExtension);
}
