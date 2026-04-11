namespace PrompterOne.Core.AI.Models;

public sealed class AgentRuntimeSettings
{
    public string ApiKey { get; init; } = string.Empty;

    public string ApiVersion { get; init; } = string.Empty;

    public string ClientType { get; init; } = AgentClientTypes.ChatCompletions;

    public int ContextSize { get; init; }

    public string Endpoint { get; init; } = string.Empty;

    public int GpuLayers { get; init; }

    public string LocalModelPath { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public string ProviderId { get; init; } = string.Empty;

    public bool IsConfigured() =>
        ProviderId.Length > 0 &&
        (Model.Length > 0 || LocalModelPath.Length > 0) &&
        (RequiresApiKey() ? ApiKey.Length > 0 : true);

    public AgentRuntimeSettings Normalize() =>
        new()
        {
            ApiKey = ApiKey.Trim(),
            ApiVersion = ApiVersion.Trim(),
            ClientType = AgentClientTypes.Normalize(ClientType),
            ContextSize = Math.Max(0, ContextSize),
            Endpoint = Endpoint.Trim(),
            GpuLayers = Math.Max(0, GpuLayers),
            LocalModelPath = LocalModelPath.Trim(),
            Model = Model.Trim(),
            ProviderId = AgentProviderIds.Normalize(ProviderId)
        };

    private bool RequiresApiKey() =>
        !string.Equals(ProviderId, AgentProviderIds.Ollama, StringComparison.Ordinal) &&
        !string.Equals(ProviderId, AgentProviderIds.LlamaSharp, StringComparison.Ordinal);
}
