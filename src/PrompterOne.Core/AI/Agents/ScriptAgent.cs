namespace PrompterOne.Core.AI.Agents;

public abstract class ScriptAgent
{
    public abstract string Id { get; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    protected abstract string SystemPrompt { get; }

    public virtual IReadOnlyList<string> SkillIds => [];

    public string GetSystemPrompt() => SystemPrompt.Trim();
}
