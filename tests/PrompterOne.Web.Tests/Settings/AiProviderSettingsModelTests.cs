using Microsoft.Extensions.Configuration;
using PrompterOne.Shared.Pages;
using PrompterOne.Shared.Settings.Models;
using PrompterOne.Shared.Settings.Services;

namespace PrompterOne.Web.Tests;

public sealed class AiProviderSettingsModelTests
{
    private const string AnthropicApiKey = "sk-ant-test";
    private const string AnthropicLegacyModel = "claude-opus-4-6";
    private const string AzureApiKey = "azure-key";
    private const string AzureDeployment = "gpt-4.1-mini";
    private const string AzureEndpoint = "https://example.openai.azure.com";
    private const string LlamaLegacyModelPath = "/models/llama-3.2-8b-instruct.gguf";
    private const string OllamaEndpoint = "http://localhost:11434";
    private const string OllamaLegacyModel = "llama3.2:8b";
    private const string OpenAiApiKey = "sk-live-openai";
    private const string OpenAiLegacyModel = "gpt-4.1-mini";

    [Test]
    public void Normalize_MigratesLegacySingleModelFields_IntoProviderModelCatalogs()
    {
        var settings = new AiProviderSettings
        {
            ClaudeApi = new AnthropicAiProviderSettings
            {
                ApiKey = AnthropicApiKey,
                Model = AnthropicLegacyModel,
                Models = []
            },
            OpenAi = new OpenAiProviderSettings
            {
                ApiKey = OpenAiApiKey,
                Model = OpenAiLegacyModel,
                Models = []
            },
            AzureOpenAi = new AzureOpenAiProviderSettings
            {
                ApiKey = AzureApiKey,
                Deployment = AzureDeployment,
                Endpoint = AzureEndpoint,
                Models = []
            },
            Ollama = new OllamaAiProviderSettings
            {
                Endpoint = OllamaEndpoint,
                Model = OllamaLegacyModel,
                Models = []
            },
            LlamaSharp = new LlamaSharpProviderSettings
            {
                ModelPath = LlamaLegacyModelPath,
                Models = []
            }
        }.Normalize();

        Assert.Contains(settings.ClaudeApi.Models, model => model.Name == AnthropicLegacyModel);
        Assert.Contains(settings.OpenAi.Models, model => model.Name == OpenAiLegacyModel);
        Assert.Contains(settings.AzureOpenAi.Models, model => model.Name == AzureDeployment);
        Assert.Contains(settings.Ollama.Models, model => model.Name == OllamaLegacyModel);
        Assert.Contains(
            settings.LlamaSharp.Models,
            model =>
                model.ModelPath == LlamaLegacyModelPath &&
                model.Name == "llama-3.2-8b-instruct");
        Assert.Equal(LlamaLegacyModelPath, settings.LlamaSharp.ModelPath);
        Assert.True(settings.HasConfiguredProvider());
    }

    [Test]
    public void CreateDefault_Normalize_StartsEmptyWithoutHardcodedModels()
    {
        var settings = AiProviderSettings.CreateDefault().Normalize();

        Assert.Equal(string.Empty, settings.ActiveProviderId);
        Assert.Empty(settings.ClaudeApi.Models);
        Assert.Empty(settings.OpenAi.Models);
        Assert.Empty(settings.AzureOpenAi.Models);
        Assert.Empty(settings.Ollama.Models);
        Assert.Empty(settings.LlamaSharp.Models);
        Assert.False(settings.HasConfiguredProvider());
    }

    [Test]
    public void Normalize_UsesOneActiveProvider_ForRuntimeSelection()
    {
        var settings = new AiProviderSettings
        {
            ActiveProviderId = SettingsAiProviderIds.OpenAi,
            ClaudeApi = new AnthropicAiProviderSettings
            {
                ApiKey = AnthropicApiKey,
                Models = [AiProviderModelSettings.Create(AnthropicLegacyModel)]
            },
            OpenAi = new OpenAiProviderSettings
            {
                ApiKey = OpenAiApiKey,
                Models = [AiProviderModelSettings.Create(OpenAiLegacyModel)]
            }
        }.Normalize();

        Assert.True(settings.HasConfiguredProvider());
        Assert.True(settings.IsActiveProvider(SettingsAiProviderIds.OpenAi));
        Assert.False(settings.IsActiveProvider(SettingsAiProviderIds.ClaudeApi));
    }

    [Test]
    public void Normalize_PreservesExplicitModelNames_WhenCatalogEntriesAlreadyExist()
    {
        const string customModelName = "custom-writing-model";

        var settings = new AiProviderSettings
        {
            OpenAi = new OpenAiProviderSettings
            {
                ApiKey = OpenAiApiKey,
                Models = [AiProviderModelSettings.Create(customModelName)]
            }
        }.Normalize();

        var model = Assert.Single(settings.OpenAi.Models);
        Assert.Equal(customModelName, model.Name);
    }

    [Test]
    public void AppSettingsFactory_MapsMinimalAzureOpenAiEndpointAndDeploymentIntoProviderSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AiProvider:Type"] = "AzureOpenAI",
                ["AiProvider:Endpoint"] = AzureEndpoint,
                ["AiProvider:ApiKey"] = AzureApiKey,
                ["AiProvider:Deployment"] = AzureDeployment
            })
            .Build();

        var settings = AiProviderAppSettingsFactory.Create(configuration);

        Assert.NotNull(settings);
        Assert.True(settings.IsActiveProvider(SettingsAiProviderIds.AzureOpenAi));
        Assert.True(settings.AzureOpenAi.IsConfigured());
        Assert.Equal(AzureApiKey, settings.AzureOpenAi.ApiKey);
        Assert.Equal(AzureEndpoint, settings.AzureOpenAi.Endpoint);
        Assert.Equal(AzureDeployment, settings.AzureOpenAi.Deployment);
        Assert.Equal(AzureDeployment, settings.AzureOpenAi.Models.Single().Name);
    }
}
