using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace PrompterOne.Shared.Services;

internal delegate Task CrossTabMessageReceivedHandler(CrossTabMessageEnvelope message);

internal sealed class CrossTabMessageBus(IJSRuntime jsRuntime, ILogger<CrossTabMessageBus>? logger = null) : IDisposable, IAsyncDisposable
{
    private const string GuidFormat = "N";
    private const string PublishFailureLogTemplate = "Failed to publish cross-tab message {MessageType}.";
    private const string PublishSkippedLogTemplate = "Skipping cross-tab publish for {MessageType} because BroadcastChannel is unavailable.";
    private const string ReceiveLogTemplate = "Received cross-tab message {MessageType} from {SourceInstanceId}.";
    private const string StartFailureLogMessage = "Failed to initialize cross-tab messaging.";
    private const string StartLogTemplate = "Cross-tab messaging initialized. Available: {IsAvailable}.";
    private const string SubscriberFailureLogTemplate = "A cross-tab subscriber failed while handling {MessageType}.";

    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly ILogger<CrossTabMessageBus> _logger = logger ?? NullLogger<CrossTabMessageBus>.Instance;
    private readonly SemaphoreSlim _startGate = new(1, 1);

    private DotNetObjectReference<CrossTabMessageBus>? _objectReference;
    private bool _disposed;
    private bool _isAvailable;
    private bool _isStarted;

    public event CrossTabMessageReceivedHandler? MessageReceived;

    public string InstanceId { get; } = Guid.NewGuid().ToString(GuidFormat);

    public bool IsAvailable => _isAvailable;

    public Task<bool> StartAsync(CancellationToken cancellationToken = default) =>
        EnsureStartedAsync(cancellationToken);

    public async Task PublishAsync<TPayload>(
        string messageType,
        TPayload payload,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);

        var isAvailable = await EnsureStartedAsync(cancellationToken);
        if (!isAvailable)
        {
            _logger.LogDebug(PublishSkippedLogTemplate, messageType);
            return;
        }

        var envelope = CrossTabMessageEnvelope.Create(messageType, InstanceId, payload);

        try
        {
            await _jsRuntime.InvokeVoidAsync(
                CrossTabInteropMethodNames.Publish,
                cancellationToken,
                CrossTabMessagingDefaults.ChannelName,
                envelope);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, PublishFailureLogTemplate, messageType);
        }
    }

    [JSInvokable]
    public async Task ReceiveAsync(CrossTabMessageEnvelope? message)
    {
        if (message is null ||
            string.IsNullOrWhiteSpace(message.MessageType) ||
            string.Equals(message.SourceInstanceId, InstanceId, StringComparison.Ordinal))
        {
            return;
        }

        _logger.LogDebug(ReceiveLogTemplate, message.MessageType, message.SourceInstanceId);
        await NotifyMessageReceivedAsync(message);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _objectReference?.Dispose();
        _objectReference = null;
        _startGate.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var objectReference = _objectReference;
        _objectReference = null;

        if (_isStarted && _isAvailable && objectReference is not null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(
                    CrossTabInteropMethodNames.Dispose,
                    CrossTabMessagingDefaults.ChannelName,
                    objectReference);
            }
            catch (JSException)
            {
            }
        }

        objectReference?.Dispose();
        _startGate.Dispose();
    }

    private async Task<bool> EnsureStartedAsync(CancellationToken cancellationToken)
    {
        if (_isStarted)
        {
            return _isAvailable;
        }

        await _startGate.WaitAsync(cancellationToken);

        try
        {
            if (_isStarted)
            {
                return _isAvailable;
            }

            if (_disposed)
            {
                return false;
            }

            _objectReference ??= DotNetObjectReference.Create(this);

            try
            {
                _isAvailable = await _jsRuntime.InvokeAsync<bool>(
                    CrossTabInteropMethodNames.Initialize,
                    cancellationToken,
                    CrossTabMessagingDefaults.ChannelName,
                    _objectReference);
                _logger.LogInformation(StartLogTemplate, _isAvailable);
            }
            catch (Exception exception)
            {
                _isAvailable = false;
                _logger.LogWarning(exception, StartFailureLogMessage);
            }

            _isStarted = true;
            return _isAvailable;
        }
        finally
        {
            _startGate.Release();
        }
    }

    private async Task NotifyMessageReceivedAsync(CrossTabMessageEnvelope message)
    {
        var handlers = MessageReceived;
        if (handlers is null)
        {
            return;
        }

        foreach (CrossTabMessageReceivedHandler handler in handlers.GetInvocationList())
        {
            try
            {
                await handler(message);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, SubscriberFailureLogTemplate, message.MessageType);
            }
        }
    }
}
