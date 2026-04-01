using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PrompterLive.Core.Abstractions;
using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Workspace;
using PrompterLive.Shared.Storage;

namespace PrompterLive.Shared.Services;

public sealed class AppBootstrapper(
    IScriptSessionService sessionService,
    IScriptRepository scriptRepository,
    ILibraryFolderRepository libraryFolderRepository,
    IMediaSceneService mediaSceneService,
    BrowserSettingsStore settingsStore,
    ILogger<AppBootstrapper>? logger = null)
{
    private readonly IScriptSessionService _sessionService = sessionService;
    private readonly IScriptRepository _scriptRepository = scriptRepository;
    private readonly ILibraryFolderRepository _libraryFolderRepository = libraryFolderRepository;
    private readonly IMediaSceneService _mediaSceneService = mediaSceneService;
    private readonly BrowserSettingsStore _settingsStore = settingsStore;
    private readonly ILogger<AppBootstrapper> _logger = logger ?? NullLogger<AppBootstrapper>.Instance;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _initialized;

    public async Task EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            _logger.LogDebug("App bootstrap requested after initialization; skipping.");
            return;
        }

        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (_initialized)
            {
                _logger.LogDebug("App bootstrap completed while waiting on the gate; skipping.");
                return;
            }

            _logger.LogInformation("Initializing PrompterLive browser state.");
            await _libraryFolderRepository.InitializeAsync(RuntimeLibrarySeedCatalog.CreateFolders(), cancellationToken);
            await _scriptRepository.InitializeAsync(RuntimeLibrarySeedCatalog.CreateDocuments(), cancellationToken);
            await _sessionService.InitializeAsync(cancellationToken);

            var readerSettings = await _settingsStore.LoadAsync<ReaderSettings>(BrowserAppSettingsKeys.ReaderSettings, cancellationToken);
            if (readerSettings is not null)
            {
                _logger.LogInformation("Restoring reader settings from browser storage.");
                await _sessionService.UpdateReaderSettingsAsync(readerSettings);
            }

            var learnSettings = await _settingsStore.LoadAsync<LearnSettings>(BrowserAppSettingsKeys.LearnSettings, cancellationToken);
            if (learnSettings is not null)
            {
                _logger.LogInformation("Restoring learn settings from browser storage.");
                await _sessionService.UpdateLearnSettingsAsync(learnSettings);
            }

            var mediaScene = await _settingsStore.LoadAsync<MediaSceneState>(BrowserAppSettingsKeys.SceneSettings, cancellationToken);
            if (mediaScene is not null)
            {
                _logger.LogInformation("Restoring media scene from browser storage.");
                _mediaSceneService.ApplyState(mediaScene);
            }

            _initialized = true;
            _logger.LogInformation("PrompterLive bootstrap completed.");
        }
        finally
        {
            _gate.Release();
        }
    }
}
