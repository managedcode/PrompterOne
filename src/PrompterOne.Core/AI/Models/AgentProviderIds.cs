namespace PrompterOne.Core.AI.Models;

public static class AgentProviderIds
{
    public const string Anthropic = "anthropic";
    public const string AzureOpenAi = "azure-openai";
    public const string LlamaSharp = "llama-sharp";
    public const string Ollama = "ollama";
    public const string OpenAi = "openai";

    public static string Normalize(string? value) =>
        value switch
        {
            Anthropic => Anthropic,
            AzureOpenAi => AzureOpenAi,
            LlamaSharp => LlamaSharp,
            Ollama => Ollama,
            _ => OpenAi
        };
}
