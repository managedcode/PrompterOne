using Microsoft.Agents.AI;
using PrompterOne.Core.AI.Abstractions;
using PrompterOne.Core.AI.Models;

namespace PrompterOne.Core.AI.Workflows;

public abstract class ScriptWorkflow
{
    public abstract string Id { get; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract ScriptWorkflowKind Kind { get; }

    public abstract IReadOnlyList<string> AgentIds { get; }

    public async Task<AIAgent> CreateWorkflowAgentAsync(
        IScriptAgentFactory agentFactory,
        ScriptAgentContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var participants = await agentFactory.CreateRequiredAsync(AgentIds, context, cancellationToken);
        return BuildWorkflowAgent(participants);
    }

    protected abstract AIAgent BuildWorkflowAgent(IReadOnlyList<AIAgent> agents);
}
