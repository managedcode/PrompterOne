using System.Text.Json;

namespace PrompterOne.Shared.Services;

internal sealed record CrossTabMessageEnvelope(
    string MessageId,
    string MessageType,
    string SourceInstanceId,
    long PublishedAtUnixTimeMilliseconds,
    string PayloadJson)
{
    private const string GuidFormat = "N";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static CrossTabMessageEnvelope Create<TPayload>(
        string messageType,
        string sourceInstanceId,
        TPayload payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceInstanceId);

        return new CrossTabMessageEnvelope(
            Guid.NewGuid().ToString(GuidFormat),
            messageType,
            sourceInstanceId,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            JsonSerializer.Serialize(payload, JsonOptions));
    }

    public TPayload? DeserializePayload<TPayload>() =>
        string.IsNullOrWhiteSpace(PayloadJson)
            ? default
            : JsonSerializer.Deserialize<TPayload>(PayloadJson, JsonOptions);
}
