using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManagedCode.Tps.Models;

public sealed class TpsSeverityJsonConverter : JsonConverter<TpsSeverity>
{
    public override TpsSeverity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString()?.ToLowerInvariant() switch
        {
            "info" => TpsSeverity.Info,
            "warning" => TpsSeverity.Warning,
            "error" => TpsSeverity.Error,
            var value => throw new JsonException($"Unsupported TPS severity '{value}'.")
        };

    public override void Write(Utf8JsonWriter writer, TpsSeverity value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            TpsSeverity.Info => "info",
            TpsSeverity.Warning => "warning",
            TpsSeverity.Error => "error",
            _ => throw new JsonException($"Unsupported TPS severity '{value}'.")
        });
    }
}
