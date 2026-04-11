using Microsoft.Agents.AI;
using PrompterOne.Core.AI.Models;

namespace PrompterOne.Core.AI.Abstractions;

public interface IScriptAgentFactory
{
    Task<AIAgent> CreateRequiredAsync(
        string agentId,
        ScriptAgentContext? context = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AIAgent>> CreateRequiredAsync(
        IEnumerable<string> agentIds,
        ScriptAgentContext? context = null,
        CancellationToken cancellationToken = default);
}
