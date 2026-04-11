using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using PrompterOne.Core.AI.Agents;
using PrompterOne.Core.AI.Models;

namespace PrompterOne.Core.AI.Workflows;

public sealed class WriterReviewSequentialWorkflow : ScriptWorkflow
{
    public const string WorkflowId = "writer-review";

    private static readonly IReadOnlyList<string> WorkflowAgentIds =
    [
        WriterScriptAgent.AgentId,
        ReviewerScriptAgent.AgentId
    ];

    public override string Id => WorkflowId;

    public override string Name => "Writer Review";

    public override string Description => "Writer drafts the script and reviewer improves it.";

    public override ScriptWorkflowKind Kind => ScriptWorkflowKind.Sequential;

    public override IReadOnlyList<string> AgentIds => WorkflowAgentIds;

    protected override AIAgent BuildWorkflowAgent(IReadOnlyList<AIAgent> agents)
    {
        var workflow = AgentWorkflowBuilder.BuildSequential(Id, agents);
        return workflow.AsAIAgent(
            Id,
            Name,
            Description,
            InProcessExecution.Lockstep,
            includeExceptionDetails: true,
            includeWorkflowOutputsInResponse: true);
    }
}
