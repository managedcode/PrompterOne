using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using PrompterOne.Core.Abstractions;

namespace PrompterOne.Shared.Services;

internal sealed class BrowserSettingsStore : IUserSettingsStore, IDisposable, IBrowserSettingsChangeNotifier
{
    private const string CrossTabEventFailureLogTemplate = "A browser settings subscriber failed while handling {Key}.";
    private const string LoadFailureLogTemplate = "Failed to load browser setting {Key}.";
    private const string RemoveFailureLogTemplate = "Failed to remove browser setting {Key}.";
    private const string SaveFailureLogTemplate = "Failed to save browser setting {Key}.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly CrossTabMessageBus _crossTabMessageBus;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<BrowserSettingsStore> _logger;
    private readonly SemaphoreSlim _startGate = new(1, 1);

    private bool _crossTabReady;
    private bool _disposed;

    public BrowserSettingsStore(
        IJSRuntime jsRuntime,
        CrossTabMessageBus crossTabMessageBus,
        ILogger<BrowserSettingsStore>? logger = null)
    {
        _jsRuntime = jsRuntime;
        _crossTabMessageBus = crossTabMessageBus;
        _logger = logger ?? NullLogger<BrowserSettingsStore>.Instance;
        _crossTabMessageBus.MessageReceived += HandleCrossTabMessageAsync;
    }

    public event BrowserSettingChangedHandler? Changed;

    public async Task<T?> LoadAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        await EnsureCrossTabReadyAsync(cancellationToken);

        try
        {
            _logger.LogDebug("Loading browser setting {Key}.", key);
            var json = await _jsRuntime.InvokeAsync<string?>(
                BrowserStorageMethodNames.LoadSettingJson,
                cancellationToken,
                ResolveStorageKey(key));

            return string.IsNullOrWhiteSpace(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, LoadFailureLogTemplate, key);
            throw;
        }
    }

    public async Task SaveAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        await EnsureCrossTabReadyAsync(cancellationToken);

        try
        {
            _logger.LogDebug("Saving browser setting {Key}.", key);
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _jsRuntime.InvokeVoidAsync(
                BrowserStorageMethodNames.SaveSettingJson,
                cancellationToken,
                ResolveStorageKey(key),
                json);

            await PublishChangedAsync(key, BrowserSettingChangeKinds.Saved, cancellationToken);
            await NotifyChangedAsync(new BrowserSettingChangeNotification(key, IsRemote: false));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, SaveFailureLogTemplate, key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await EnsureCrossTabReadyAsync(cancellationToken);

        try
        {
            _logger.LogDebug("Removing browser setting {Key}.", key);
            await _jsRuntime.InvokeVoidAsync(
                BrowserStorageMethodNames.RemoveStorageValue,
                cancellationToken,
                ResolveStorageKey(key));

            await PublishChangedAsync(key, BrowserSettingChangeKinds.Removed, cancellationToken);
            await NotifyChangedAsync(new BrowserSettingChangeNotification(key, IsRemote: false));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, RemoveFailureLogTemplate, key);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _crossTabMessageBus.MessageReceived -= HandleCrossTabMessageAsync;
        _startGate.Dispose();
    }

    private async Task EnsureCrossTabReadyAsync(CancellationToken cancellationToken)
    {
        if (_crossTabReady)
        {
            return;
        }

        await _startGate.WaitAsync(cancellationToken);

        try
        {
            if (_crossTabReady)
            {
                return;
            }

            await _crossTabMessageBus.StartAsync(cancellationToken);
            _crossTabReady = true;
        }
        finally
        {
            _startGate.Release();
        }
    }

    private async Task HandleCrossTabMessageAsync(CrossTabMessageEnvelope message)
    {
        if (!string.Equals(message.MessageType, CrossTabMessageTypes.SettingsChanged, StringComparison.Ordinal))
        {
            return;
        }

        var payload = message.DeserializePayload<BrowserSettingChangePayload>();
        if (payload is null || string.IsNullOrWhiteSpace(payload.Key))
        {
            return;
        }

        await NotifyChangedAsync(new BrowserSettingChangeNotification(payload.Key, IsRemote: true));
    }

    private async Task NotifyChangedAsync(BrowserSettingChangeNotification notification)
    {
        var handlers = Changed;
        if (handlers is null)
        {
            return;
        }

        foreach (BrowserSettingChangedHandler handler in handlers.GetInvocationList())
        {
            try
            {
                await handler(notification);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, CrossTabEventFailureLogTemplate, notification.Key);
            }
        }
    }

    private Task PublishChangedAsync(
        string key,
        string changeKind,
        CancellationToken cancellationToken)
    {
        return _crossTabMessageBus.PublishAsync(
            CrossTabMessageTypes.SettingsChanged,
            new BrowserSettingChangePayload(key, changeKind),
            cancellationToken);
    }

    private static string ResolveStorageKey(string key) =>
        string.Concat(BrowserStorageKeys.SettingsPrefix, key);
}
