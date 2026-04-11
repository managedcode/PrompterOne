namespace PrompterOne.Core.AI.Models;

public sealed record ScriptAgentRunResult(
    string WorkflowId,
    string WorkflowName,
    string Input,
    IReadOnlyList<ScriptAgentStepResult> Steps,
    string Output);
