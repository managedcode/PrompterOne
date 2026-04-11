namespace PrompterOne.Core.AI.Models;

public static class AgentClientTypes
{
    public const string Assistants = "assistants";
    public const string ChatCompletions = "chat-completions";
    public const string Responses = "responses";

    public static string Normalize(string? value) =>
        value switch
        {
            Assistants => Assistants,
            Responses => Responses,
            _ => ChatCompletions
        };
}
