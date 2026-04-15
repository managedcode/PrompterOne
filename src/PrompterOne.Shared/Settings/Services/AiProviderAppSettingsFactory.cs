using Microsoft.Extensions.Configuration;
using PrompterOne.Shared.Pages;
using PrompterOne.Shared.Settings.Models;

namespace PrompterOne.Shared.Settings.Services;

internal static class AiProviderAppSettingsFactory
{
    private const string AiProviderSectionName = "AiProvider";
    private const string AlternateAiProviderSectionName = "AI";
    private const string ApiKeyKey = "ApiKey";
    private const string AzureOpenAiSectionName = "AzureOpenAi";
    private const string BaseUrlKey = "BaseUrl";
    private const string DeploymentKey = "Deployment";
    private const string DeploymentIdKey = "DeploymentId";
    private const string DeploymentsSectionName = "Deployments";
    private const string EndpointKey = "Endpoint";
    private const string EndpointsSectionName = "Endpoints";
    private const string ModelKey = "Model";
    private const string ModelsSectionName = "Models";
    private const string TypeKey = "Type";

    public static AiProviderSettings? Create(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = ResolveAiProviderSection(configuration);
        if (section is null)
        {
            return null;
        }

        var settings = new AiProviderSettings();
        var providerId = ResolveProviderId(section);
        switch (providerId)
        {
            case SettingsAiProviderIds.AzureOpenAi:
                settings.AzureOpenAi = CreateAzureOpenAiSettings(section);
                break;
            case SettingsAiProviderIds.ClaudeApi:
                settings.ClaudeApi = CreateAnthropicSettings(section);
                break;
            case SettingsAiProviderIds.Ollama:
                settings.Ollama = CreateOllamaSettings(section);
                break;
            case SettingsAiProviderIds.OpenAi:
                settings.OpenAi = CreateOpenAiSettings(section);
                break;
            default:
                return null;
        }

        settings.ActiveProviderId = providerId;
        settings.Normalize();
        return settings.HasConfiguredProvider() ? settings : null;
    }

    private static AnthropicAiProviderSettings CreateAnthropicSettings(IConfigurationSection section) =>
        new()
        {
            ApiKey = section[ApiKeyKey] ?? string.Empty,
            BaseUrl = section[BaseUrlKey] ?? section[EndpointKey] ?? string.Empty,
            Models = CreateModels(section, ModelKey)
        };

    private static AzureOpenAiProviderSettings CreateAzureOpenAiSettings(IConfigurationSection section)
    {
        var endpointSection = ResolveEndpointSection(section);
        return new AzureOpenAiProviderSettings
        {
            ApiKey = endpointSection?[ApiKeyKey] ?? section[ApiKeyKey] ?? string.Empty,
            Deployment = section[DeploymentKey] ?? section[DeploymentIdKey] ?? section[ModelKey] ?? string.Empty,
            Endpoint = endpointSection?[EndpointKey] ?? section[EndpointKey] ?? string.Empty,
            Models = CreateModels(section, DeploymentKey, DeploymentIdKey, ModelKey, DeploymentsSectionName)
        };
    }

    private static OllamaAiProviderSettings CreateOllamaSettings(IConfigurationSection section) =>
        new()
        {
            Endpoint = section[EndpointKey] ?? section[BaseUrlKey] ?? string.Empty,
            Models = CreateModels(section, ModelKey)
        };

    private static OpenAiProviderSettings CreateOpenAiSettings(IConfigurationSection section) =>
        new()
        {
            ApiKey = section[ApiKeyKey] ?? string.Empty,
            BaseUrl = section[BaseUrlKey] ?? section[EndpointKey] ?? string.Empty,
            Models = CreateModels(section, ModelKey)
        };

    private static List<AiProviderModelSettings> CreateModels(
        IConfigurationSection section,
        params string[] singularModelKeys)
    {
        var models = new List<AiProviderModelSettings>();
        foreach (var key in singularModelKeys)
        {
            AddModel(models, section[key]);
        }

        AddModelsFromSection(models, section.GetSection(ModelsSectionName));
        AddModelsFromSection(models, section.GetSection(DeploymentsSectionName));
        return models;
    }

    private static void AddModelsFromSection(List<AiProviderModelSettings> models, IConfigurationSection section)
    {
        if (!section.Exists())
        {
            return;
        }

        foreach (var modelSection in section.GetChildren())
        {
            AddModel(
                models,
                modelSection.Value ??
                modelSection[DeploymentKey] ??
                modelSection[DeploymentIdKey] ??
                modelSection[ModelKey] ??
                modelSection["Name"] ??
                modelSection.Key);
        }
    }

    private static void AddModel(List<AiProviderModelSettings> models, string? name)
    {
        var model = AiProviderModelSettings.Create(name ?? string.Empty);
        if (!model.IsConfigured() ||
            models.Any(existing => string.Equals(existing.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        models.Add(model);
    }

    private static IConfigurationSection? ResolveAiProviderSection(IConfiguration configuration)
    {
        var section = configuration.GetSection(AiProviderSectionName);
        if (section.Exists())
        {
            return section;
        }

        section = configuration.GetSection(AlternateAiProviderSectionName);
        if (section.Exists())
        {
            return section;
        }

        section = configuration.GetSection(AzureOpenAiSectionName);
        return section.Exists() ? section : null;
    }

    private static IConfigurationSection? ResolveEndpointSection(IConfigurationSection section)
    {
        var endpoints = section.GetSection(EndpointsSectionName);
        if (!endpoints.Exists())
        {
            return null;
        }

        return endpoints.GetChildren().FirstOrDefault();
    }

    private static string ResolveProviderId(IConfigurationSection section)
    {
        var type = section[TypeKey] ?? section.Key;
        return type.Trim().ToLowerInvariant().Replace(" ", string.Empty, StringComparison.Ordinal) switch
        {
            "anthropic" or "claude" or "claudeapi" => SettingsAiProviderIds.ClaudeApi,
            "azureopenai" => SettingsAiProviderIds.AzureOpenAi,
            "ollama" => SettingsAiProviderIds.Ollama,
            "openai" => SettingsAiProviderIds.OpenAi,
            _ => string.Equals(section.Key, AzureOpenAiSectionName, StringComparison.OrdinalIgnoreCase)
                ? SettingsAiProviderIds.AzureOpenAi
                : string.Empty
        };
    }
}
