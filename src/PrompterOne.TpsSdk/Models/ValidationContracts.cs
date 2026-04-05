using System.Text.Json.Serialization;

namespace ManagedCode.Tps.Models;

[JsonConverter(typeof(TpsSeverityJsonConverter))]
public enum TpsSeverity
{
    Info,
    Warning,
    Error
}

internal interface ICompiledTimeRange
{
    int StartWordIndex { get; set; }

    int EndWordIndex { get; set; }

    int StartMs { get; set; }

    int EndMs { get; set; }
}

public sealed record TpsPosition(
    [property: JsonPropertyName("line")] int Line,
    [property: JsonPropertyName("column")] int Column,
    [property: JsonPropertyName("offset")] int Offset);

public sealed record TpsRange(
    [property: JsonPropertyName("start")] TpsPosition Start,
    [property: JsonPropertyName("end")] TpsPosition End);

public sealed record TpsDiagnostic(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] TpsSeverity Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("range")] TpsRange Range,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [property: JsonPropertyName("suggestion")] string? Suggestion = null);

public class TpsValidationResult
{
    [JsonPropertyName("ok")]
    public required bool Ok { get; init; }

    [JsonPropertyName("diagnostics")]
    public required IReadOnlyList<TpsDiagnostic> Diagnostics { get; init; }
}

public sealed class TpsParseResult : TpsValidationResult
{
    [JsonPropertyName("document")]
    public required TpsDocument Document { get; init; }
}

public sealed class TpsCompilationResult : TpsValidationResult
{
    [JsonPropertyName("document")]
    public required TpsDocument Document { get; init; }

    [JsonPropertyName("script")]
    public required CompiledScript Script { get; init; }
}
