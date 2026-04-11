namespace PrompterOne.Core.AI.Models;

public sealed record ScriptAgentStepResult(
    string AgentId,
    string AgentName,
    string Input,
    string Output);
