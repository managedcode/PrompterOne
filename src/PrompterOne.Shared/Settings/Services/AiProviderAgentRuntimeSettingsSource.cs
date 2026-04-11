using PrompterOne.Core.AI.Abstractions;
using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Settings.Models;

namespace PrompterOne.Shared.Settings.Services;

public sealed class AiProviderAgentRuntimeSettingsSource(AiProviderSettingsStore settingsStore) : IAgentRuntimeSettingsSource
{
    private readonly AiProviderSettingsStore _settingsStore = settingsStore;

    public async Task<AgentRuntimeSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingsStore.LoadAsync(cancellationToken);
        return SelectProvider(settings.Normalize());
    }

    private static AgentRuntimeSettings SelectProvider(AiProviderSettings settings)
    {
        if (settings.ClaudeApi.IsConfigured())
        {
            return new AgentRuntimeSettings
            {
                ApiKey = settings.ClaudeApi.ApiKey,
                ClientType = AgentClientTypes.ChatCompletions,
                ContextSize = GetTextModel(settings.ClaudeApi.Models)?.ContextSize ?? 0,
                Endpoint = settings.ClaudeApi.BaseUrl,
                Model = GetTextModel(settings.ClaudeApi.Models)?.Name ?? settings.ClaudeApi.Model,
                ProviderId = AgentProviderIds.Anthropic
            };
        }

        if (settings.OpenAi.IsConfigured())
        {
            return new AgentRuntimeSettings
            {
                ApiKey = settings.OpenAi.ApiKey,
                ClientType = settings.OpenAi.ClientType,
                ContextSize = GetTextModel(settings.OpenAi.Models)?.ContextSize ?? 0,
                Endpoint = settings.OpenAi.BaseUrl,
                Model = GetTextModel(settings.OpenAi.Models)?.Name ?? settings.OpenAi.Model,
                ProviderId = AgentProviderIds.OpenAi
            };
        }

        if (settings.AzureOpenAi.IsConfigured())
        {
            return new AgentRuntimeSettings
            {
                ApiKey = settings.AzureOpenAi.ApiKey,
                ApiVersion = settings.AzureOpenAi.ApiVersion,
                ClientType = settings.AzureOpenAi.ClientType,
                ContextSize = GetTextModel(settings.AzureOpenAi.Models)?.ContextSize ?? 0,
                Endpoint = settings.AzureOpenAi.Endpoint,
                Model = GetTextModel(settings.AzureOpenAi.Models)?.Name ?? settings.AzureOpenAi.Deployment,
                ProviderId = AgentProviderIds.AzureOpenAi
            };
        }

        if (settings.Ollama.IsConfigured())
        {
            return new AgentRuntimeSettings
            {
                ClientType = AgentClientTypes.ChatCompletions,
                ContextSize = GetTextModel(settings.Ollama.Models)?.ContextSize ?? 0,
                Endpoint = settings.Ollama.Endpoint,
                Model = GetTextModel(settings.Ollama.Models)?.Name ?? settings.Ollama.Model,
                ProviderId = AgentProviderIds.Ollama
            };
        }

        if (settings.LlamaSharp.IsConfigured())
        {
            var model = GetTextModel(settings.LlamaSharp.Models);
            return new AgentRuntimeSettings
            {
                ClientType = AgentClientTypes.ChatCompletions,
                ContextSize = model?.ContextSize ?? settings.LlamaSharp.ContextSize,
                GpuLayers = settings.LlamaSharp.GpuLayers,
                LocalModelPath = model?.ModelPath ?? settings.LlamaSharp.ModelPath,
                Model = model?.Name ?? string.Empty,
                ProviderId = AgentProviderIds.LlamaSharp
            };
        }

        return new AgentRuntimeSettings();
    }

    private static AiProviderModelSettings? GetTextModel(IEnumerable<AiProviderModelSettings> models) =>
        models.FirstOrDefault(static model => model.IsConfigured() && string.Equals(model.Type, AiProviderModelTypes.Text, StringComparison.Ordinal))
        ?? models.FirstOrDefault(static model => model.IsConfigured());
}
