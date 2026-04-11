using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using PrompterOne.Core.AI.Agents;
using PrompterOne.Core.AI.Models;

namespace PrompterOne.Core.AI.Workflows;

public sealed class WriterReviewGroupChatWorkflow : ScriptWorkflow
{
    public const string WorkflowId = "writer-review-group-chat";

    private static readonly IReadOnlyList<string> WorkflowAgentIds =
    [
        WriterScriptAgent.AgentId,
        ReviewerScriptAgent.AgentId
    ];

    public override string Id => WorkflowId;

    public override string Name => "Writer Review Group Chat";

    public override string Description => "Writer and reviewer collaborate in a round-robin group chat.";

    public override ScriptWorkflowKind Kind => ScriptWorkflowKind.GroupChat;

    public override IReadOnlyList<string> AgentIds => WorkflowAgentIds;

    protected override AIAgent BuildWorkflowAgent(IReadOnlyList<AIAgent> agents)
    {
        var workflow = AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(participants =>
                new RoundRobinGroupChatManager(participants)
                {
                    MaximumIterationCount = 4
                })
            .AddParticipants(agents.ToArray())
            .Build();

        return workflow.AsAIAgent(
            Id,
            Name,
            Description,
            InProcessExecution.Lockstep,
            includeExceptionDetails: true,
            includeWorkflowOutputsInResponse: true);
    }
}
