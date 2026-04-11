using PrompterOne.Core.AI.Models;

namespace PrompterOne.Core.AI.Abstractions;

public interface IAgentRuntimeSettingsSource
{
    Task<AgentRuntimeSettings> LoadAsync(CancellationToken cancellationToken = default);
}
