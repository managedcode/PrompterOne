using PrompterOne.Shared.Pages;

namespace PrompterOne.Shared.Settings.Models;

public sealed class AiProviderSettings
{
    public const string StorageKey = "prompterone.ai-providers";

    public string ActiveProviderId { get; set; } = string.Empty;

    public AnthropicAiProviderSettings ClaudeApi { get; set; } = new();

    public AzureOpenAiProviderSettings AzureOpenAi { get; set; } = new();

    public LlamaSharpProviderSettings LlamaSharp { get; set; } = new();

    public OllamaAiProviderSettings Ollama { get; set; } = new();

    public OpenAiProviderSettings OpenAi { get; set; } = new();

    public static AiProviderSettings CreateDefault() => new();

    public AiProviderSettings Normalize()
    {
        ClaudeApi = (ClaudeApi ?? new AnthropicAiProviderSettings()).Normalize();
        AzureOpenAi = (AzureOpenAi ?? new AzureOpenAiProviderSettings()).Normalize();
        LlamaSharp = (LlamaSharp ?? new LlamaSharpProviderSettings()).Normalize();
        OpenAi = (OpenAi ?? new OpenAiProviderSettings()).Normalize();
        Ollama = (Ollama ?? new OllamaAiProviderSettings()).Normalize();
        ActiveProviderId = NormalizeActiveProviderId(ActiveProviderId);
        return this;
    }

    public bool HasConfiguredProvider() =>
        IsProviderConfigured(ActiveProviderId);

    public bool IsActiveProvider(string providerId) =>
        string.Equals(ActiveProviderId, providerId, StringComparison.Ordinal);

    public bool IsProviderConfigured(string providerId) =>
        providerId switch
        {
            SettingsAiProviderIds.ClaudeApi => ClaudeApi.IsConfigured(),
            SettingsAiProviderIds.AzureOpenAi => AzureOpenAi.IsConfigured(),
            SettingsAiProviderIds.LlamaSharp => LlamaSharp.IsConfigured(),
            SettingsAiProviderIds.Ollama => Ollama.IsConfigured(),
            SettingsAiProviderIds.OpenAi => OpenAi.IsConfigured(),
            _ => false
        };

    private string NormalizeActiveProviderId(string? providerId)
    {
        var normalized = SettingsAiProviderIds.Normalize(providerId);
        if (!string.IsNullOrWhiteSpace(normalized) && IsProviderConfigured(normalized))
        {
            return normalized;
        }

        return EnumerateProviderIds().FirstOrDefault(IsProviderConfigured) ?? string.Empty;
    }

    private static IEnumerable<string> EnumerateProviderIds()
    {
        yield return SettingsAiProviderIds.ClaudeApi;
        yield return SettingsAiProviderIds.OpenAi;
        yield return SettingsAiProviderIds.AzureOpenAi;
        yield return SettingsAiProviderIds.Ollama;
        yield return SettingsAiProviderIds.LlamaSharp;
    }
}

public sealed class AnthropicAiProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public List<AiProviderModelSettings> Models { get; set; } = [];

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        Models.Any(static model => model.IsConfigured());

    public AnthropicAiProviderSettings Normalize()
    {
        ApiKey = ApiKey.Trim();
        BaseUrl = BaseUrl.Trim();
        Models = AiProviderSettingsModelCatalog.Normalize(Models, Model);
        Model = AiProviderSettingsModelCatalog.GetPrimaryModelName(Models);
        return this;
    }
}

public sealed class OpenAiProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public List<AiProviderModelSettings> Models { get; set; } = [];

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        Models.Any(static model => model.IsConfigured());

    public OpenAiProviderSettings Normalize()
    {
        ApiKey = ApiKey.Trim();
        BaseUrl = BaseUrl.Trim();
        Models = AiProviderSettingsModelCatalog.Normalize(Models, Model);
        Model = AiProviderSettingsModelCatalog.GetPrimaryModelName(Models);
        return this;
    }
}

public sealed class AzureOpenAiProviderSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = string.Empty;

    public string Deployment { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public List<AiProviderModelSettings> Models { get; set; } = [];

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(Endpoint) &&
        Models.Any(static model => model.IsConfigured());

    public AzureOpenAiProviderSettings Normalize()
    {
        ApiKey = ApiKey.Trim();
        ApiVersion = ApiVersion.Trim();
        Deployment = Deployment.Trim();
        Endpoint = Endpoint.Trim();
        Models = AiProviderSettingsModelCatalog.Normalize(Models, Deployment);
        Deployment = AiProviderSettingsModelCatalog.GetPrimaryModelName(Models);
        return this;
    }
}

public sealed class OllamaAiProviderSettings
{
    public string Endpoint { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public List<AiProviderModelSettings> Models { get; set; } = [];

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        Models.Any(static model => model.IsConfigured());

    public OllamaAiProviderSettings Normalize()
    {
        Endpoint = Endpoint.Trim();
        Models = AiProviderSettingsModelCatalog.Normalize(Models, Model);
        Model = AiProviderSettingsModelCatalog.GetPrimaryModelName(Models);
        return this;
    }
}

public sealed class LlamaSharpProviderSettings
{
    public string ModelPath { get; set; } = string.Empty;

    public List<AiProviderModelSettings> Models { get; set; } = [];

    public bool IsConfigured() =>
        Models.Any(static model => model.IsConfiguredWithLocalPath());

    public LlamaSharpProviderSettings Normalize()
    {
        ModelPath = ModelPath.Trim();
        Models = AiProviderSettingsModelCatalog.Normalize(Models, modelPath: ModelPath);
        ModelPath = AiProviderSettingsModelCatalog.GetPrimaryModelPath(Models);
        return this;
    }
}

internal static class AiProviderSettingsModelCatalog
{
    public static List<AiProviderModelSettings> Normalize(
        List<AiProviderModelSettings>? models,
        string? legacyModelName = null,
        string? modelPath = null)
    {
        var normalizedModels = (models ?? [])
            .Select(static model => (model ?? new AiProviderModelSettings()).Normalize())
            .Where(static model => model.HasAnyValue())
            .ToList();

        var legacyModel = AiProviderModelSettings.Create(legacyModelName ?? string.Empty, modelPath ?? string.Empty);
        if (legacyModel.HasAnyValue() && !ContainsModel(normalizedModels, legacyModel))
        {
            normalizedModels.Add(legacyModel);
        }

        return normalizedModels;
    }

    public static string GetPrimaryModelName(IReadOnlyList<AiProviderModelSettings> models) =>
        models.FirstOrDefault(static model => model.IsConfigured())?.Name
        ?? string.Empty;

    public static string GetPrimaryModelPath(IReadOnlyList<AiProviderModelSettings> models) =>
        models.FirstOrDefault(static model => model.IsConfiguredWithLocalPath())?.ModelPath
        ?? string.Empty;

    private static bool ContainsModel(IReadOnlyCollection<AiProviderModelSettings> models, AiProviderModelSettings candidate) =>
        models.Any(
            model =>
                string.Equals(model.Name, candidate.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(model.ModelPath, candidate.ModelPath, StringComparison.OrdinalIgnoreCase));
}
