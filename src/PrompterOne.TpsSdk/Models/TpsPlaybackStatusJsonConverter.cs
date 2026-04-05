using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManagedCode.Tps.Models;

public sealed class TpsPlaybackStatusJsonConverter : JsonConverter<TpsPlaybackStatus>
{
    public override TpsPlaybackStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString()?.ToLowerInvariant() switch
        {
            "idle" => TpsPlaybackStatus.Idle,
            "playing" => TpsPlaybackStatus.Playing,
            "paused" => TpsPlaybackStatus.Paused,
            "completed" => TpsPlaybackStatus.Completed,
            _ => throw new JsonException($"Unknown TPS playback status '{reader.GetString()}'.")
        };

    public override void Write(Utf8JsonWriter writer, TpsPlaybackStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            TpsPlaybackStatus.Idle => "idle",
            TpsPlaybackStatus.Playing => "playing",
            TpsPlaybackStatus.Paused => "paused",
            TpsPlaybackStatus.Completed => "completed",
            _ => throw new JsonException($"Unknown TPS playback status '{value}'.")
        });
    }
}
