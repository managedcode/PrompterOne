using PrompterOne.Core.AI.Abstractions;
using PrompterOne.Core.AI.Models;
using PrompterOne.Shared.Pages;
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
        return settings.ActiveProviderId switch
        {
            SettingsAiProviderIds.ClaudeApi => CreateAnthropicSettings(settings.ClaudeApi),
            SettingsAiProviderIds.OpenAi => CreateOpenAiSettings(settings.OpenAi),
            SettingsAiProviderIds.AzureOpenAi => CreateAzureOpenAiSettings(settings.AzureOpenAi),
            SettingsAiProviderIds.Ollama => CreateOllamaSettings(settings.Ollama),
            SettingsAiProviderIds.LlamaSharp => CreateLlamaSharpSettings(settings.LlamaSharp),
            _ => new AgentRuntimeSettings()
        };
    }

    private static AgentRuntimeSettings CreateAnthropicSettings(AnthropicAiProviderSettings settings)
    {
        if (settings.IsConfigured())
        {
            return new AgentRuntimeSettings
            {
                ApiKey = settings.ApiKey,
                Endpoint = settings.BaseUrl,
                Model = GetModelName(settings.Models, settings.Model),
                ProviderId = AgentProviderIds.Anthropic
            };
        }

        return new AgentRuntimeSettings();
    }

    private static AgentRuntimeSettings CreateOpenAiSettings(OpenAiProviderSettings settings)
    {
        if (settings.IsConfigured())
        {
            return new AgentRuntimeSettings
            {
                ApiKey = settings.ApiKey,
                Endpoint = settings.BaseUrl,
                Model = GetModelName(settings.Models, settings.Model),
                ProviderId = AgentProviderIds.OpenAi
            };
        }

        return new AgentRuntimeSettings();
    }

    private static AgentRuntimeSettings CreateAzureOpenAiSettings(AzureOpenAiProviderSettings settings)
    {
        if (settings.IsConfigured())
        {
            return new AgentRuntimeSettings
            {
                ApiKey = settings.ApiKey,
                ApiVersion = settings.ApiVersion,
                Endpoint = settings.Endpoint,
                Model = GetModelName(settings.Models, settings.Deployment),
                ProviderId = AgentProviderIds.AzureOpenAi
            };
        }

        return new AgentRuntimeSettings();
    }

    private static AgentRuntimeSettings CreateOllamaSettings(OllamaAiProviderSettings settings)
    {
        if (settings.IsConfigured())
        {
            return new AgentRuntimeSettings
            {
                Endpoint = settings.Endpoint,
                Model = GetModelName(settings.Models, settings.Model),
                ProviderId = AgentProviderIds.Ollama
            };
        }

        return new AgentRuntimeSettings();
    }

    private static AgentRuntimeSettings CreateLlamaSharpSettings(LlamaSharpProviderSettings settings)
    {
        if (settings.IsConfigured())
        {
            var model = GetModel(settings.Models);
            return new AgentRuntimeSettings
            {
                LocalModelPath = model?.ModelPath ?? settings.ModelPath,
                Model = model?.Name ?? string.Empty,
                ProviderId = AgentProviderIds.LlamaSharp
            };
        }

        return new AgentRuntimeSettings();
    }

    private static AiProviderModelSettings? GetModel(IEnumerable<AiProviderModelSettings> models) =>
        models.FirstOrDefault(static model => model.IsConfigured());

    private static string GetModelName(IEnumerable<AiProviderModelSettings> models, string fallback) =>
        GetModel(models)?.Name ?? fallback;
}
