namespace PrompterOne.Core.AI.Agents;

public sealed class WriterScriptAgent : ScriptAgent
{
    public const string AgentId = "writer";

    private static readonly IReadOnlyList<string> AgentSkillIds = [AgentId];

    public override string Id => AgentId;

    public override string Name => "Script Writer";

    public override string Description => "Drafts or rewrites script text for PrompterOne.";

    public override IReadOnlyList<string> SkillIds => AgentSkillIds;

    protected override string SystemPrompt =>
        """
        Write clean, readable script text for PrompterOne.
        Return the script draft only unless a loaded skill explicitly asks for extra notes.
        Keep the wording practical for rehearsal, RSVP, and teleprompter reading.
        """;
}
