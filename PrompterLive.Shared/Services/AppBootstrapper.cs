using PrompterLive.Core.Abstractions;
using PrompterLive.Core.Models.Media;
using PrompterLive.Core.Models.Workspace;

namespace PrompterLive.Shared.Services;

public sealed class AppBootstrapper
{
    private readonly IScriptSessionService _sessionService;
    private readonly IMediaSceneService _mediaSceneService;
    private readonly BrowserSettingsStore _settingsStore;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _initialized;

    public AppBootstrapper(
        IScriptSessionService sessionService,
        IMediaSceneService mediaSceneService,
        BrowserSettingsStore settingsStore)
    {
        _sessionService = sessionService;
        _mediaSceneService = mediaSceneService;
        _settingsStore = settingsStore;
    }

    public async Task EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (_initialized)
            {
                return;
            }

            await _sessionService.InitializeAsync(cancellationToken);

            var readerSettings = await _settingsStore.LoadAsync<ReaderSettings>("prompterlive.reader", cancellationToken);
            if (readerSettings is not null)
            {
                await _sessionService.UpdateReaderSettingsAsync(readerSettings);
            }

            var learnSettings = await _settingsStore.LoadAsync<LearnSettings>("prompterlive.learn", cancellationToken);
            if (learnSettings is not null)
            {
                await _sessionService.UpdateLearnSettingsAsync(learnSettings);
            }

            var mediaScene = await _settingsStore.LoadAsync<MediaSceneState>("prompterlive.scene", cancellationToken);
            if (mediaScene is not null)
            {
                _mediaSceneService.ApplyState(mediaScene);
            }

            _initialized = true;
        }
        finally
        {
            _gate.Release();
        }
    }
}
