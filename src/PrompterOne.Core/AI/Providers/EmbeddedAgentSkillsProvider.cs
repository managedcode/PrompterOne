#pragma warning disable MAAI001
using System.Reflection;
using Microsoft.Agents.AI;

namespace PrompterOne.Core.AI.Providers;

public sealed class EmbeddedAgentSkillsProvider
{
    private const string EmbeddedMarkdownResourceName = "embedded-markdown";
    private const string EmbeddedResourcePathName = "embedded-resource-path";
    private const string ResourcePrefix = "PrompterOne.Core.AI.Skills.";

    private static readonly Lazy<IReadOnlyDictionary<string, EmbeddedAgentSkillDocument>> SkillDocuments =
        new(LoadSkillDocuments);

    public AIContextProvider? CreateProvider(IEnumerable<string> skillIds)
    {
        var ids = skillIds
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ids.Length == 0)
        {
            return null;
        }

        var skills = new AgentInlineSkill[ids.Length];
        for (var i = 0; i < ids.Length; i++)
        {
            skills[i] = GetRequiredSkill(ids[i]).ToInlineSkill();
        }

        return new AgentSkillsProvider(skills);
    }

    private static EmbeddedAgentSkillDocument GetRequiredSkill(string skillId) =>
        SkillDocuments.Value.TryGetValue(skillId, out var skill)
            ? skill
            : throw new InvalidOperationException($"Missing embedded agent skill '{skillId}'.");

    private static IReadOnlyDictionary<string, EmbeddedAgentSkillDocument> LoadSkillDocuments()
    {
        var assembly = typeof(EmbeddedAgentSkillsProvider).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(static name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .Where(static name => name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        var documents = new Dictionary<string, EmbeddedAgentSkillDocument>(StringComparer.OrdinalIgnoreCase);
        foreach (var resourceName in resourceNames)
        {
            var document = EmbeddedAgentSkillDocument.Parse(resourceName, LoadRequiredResourceText(assembly, resourceName));
            documents[document.Id] = document;
        }

        return documents;
    }

    private static string LoadRequiredResourceText(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded agent skill resource '{resourceName}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Trim();
    }

    private sealed record EmbeddedAgentSkillDocument(
        string Id,
        string Description,
        string Compatibility,
        string Instructions,
        string RawMarkdown,
        string ResourceName)
    {
        public static EmbeddedAgentSkillDocument Parse(string resourceName, string rawMarkdown)
        {
            var normalized = rawMarkdown.Replace("\r\n", "\n", StringComparison.Ordinal);
            if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Embedded agent skill '{resourceName}' must start with YAML frontmatter.");
            }

            var closingIndex = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
            if (closingIndex < 0)
            {
                throw new InvalidOperationException($"Embedded agent skill '{resourceName}' is missing a closing YAML frontmatter marker.");
            }

            var frontmatterBlock = normalized[4..closingIndex];
            var instructions = normalized[(closingIndex + 5)..].Trim();
            var frontmatter = ParseFrontmatter(resourceName, frontmatterBlock);

            return new EmbeddedAgentSkillDocument(
                frontmatter.Id,
                frontmatter.Description,
                frontmatter.Compatibility,
                instructions,
                normalized,
                resourceName);
        }

        public AgentInlineSkill ToInlineSkill()
        {
            var frontmatter = new AgentSkillFrontmatter(Id, Description, Compatibility);
            var skill = new AgentInlineSkill(frontmatter, Instructions);
            skill.AddResource(
                EmbeddedMarkdownResourceName,
                RawMarkdown,
                "Original skill markdown embedded in PrompterOne.Core.");
            skill.AddResource(
                EmbeddedResourcePathName,
                ResourceName,
                "Assembly embedded resource name for this skill.");
            return skill;
        }

        private static SkillFrontmatter ParseFrontmatter(string resourceName, string frontmatterBlock)
        {
            string? id = null;
            string? description = null;
            var compatibility = string.Empty;

            foreach (var rawLine in frontmatterBlock.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var separatorIndex = rawLine.IndexOf(':');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = rawLine[..separatorIndex].Trim();
                var value = rawLine[(separatorIndex + 1)..].Trim();

                if (key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    id = value;
                    continue;
                }

                if (key.Equals("description", StringComparison.OrdinalIgnoreCase))
                {
                    description = value;
                    continue;
                }

                if (key.Equals("compatibility", StringComparison.OrdinalIgnoreCase))
                {
                    compatibility = value;
                }
            }

            return string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(description)
                ? throw new InvalidOperationException($"Embedded agent skill '{resourceName}' must declare both 'name' and 'description' in YAML frontmatter.")
                : new SkillFrontmatter(id.Trim(), description.Trim(), compatibility.Trim());
        }

        private sealed record SkillFrontmatter(string Id, string Description, string Compatibility);
    }
}
#pragma warning restore MAAI001
