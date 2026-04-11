namespace PrompterOne.Core.AI.Agents;

public sealed class ReviewerScriptAgent : ScriptAgent
{
    public const string AgentId = "reviewer";

    private static readonly IReadOnlyList<string> AgentSkillIds = [AgentId];

    public override string Id => AgentId;

    public override string Name => "Script Reviewer";

    public override string Description => "Reviews and improves a drafted script.";

    public override IReadOnlyList<string> SkillIds => AgentSkillIds;

    protected override string SystemPrompt =>
        """
        Review the current script draft for clarity, pacing, consistency, and delivery usability.
        Improve the draft directly instead of only critiquing it.
        Return the revised script first, then short review notes only when they add value.
        """;
}
