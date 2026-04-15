namespace PrompterOne.Shared.Pages;

public static class SettingsAiProviderIds
{
    public const string AzureOpenAi = "azure-openai";
    public const string ClaudeApi = "claude-api";
    public const string ClaudeCode = "claude-code";
    public const string Codex = "codex";
    public const string Copilot = "copilot";
    public const string LlamaSharp = "llamasharp";
    public const string Ollama = "ollama";
    public const string OpenAi = "openai";

    public static string Normalize(string? providerId) =>
        providerId?.Trim().ToLowerInvariant() switch
        {
            AzureOpenAi => AzureOpenAi,
            ClaudeApi => ClaudeApi,
            LlamaSharp => LlamaSharp,
            Ollama => Ollama,
            OpenAi => OpenAi,
            _ => string.Empty
        };
}
