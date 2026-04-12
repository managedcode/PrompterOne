using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using PrompterOne.Core.AI.Models;
using PrompterOne.Core.AI.Services;
using PrompterOne.Core.AI.Tools;

namespace PrompterOne.Core.Tests;

public sealed class ScriptAgentToolProviderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    static ScriptAgentToolProviderTests() =>
        JsonOptions.Converters.Add(new JsonStringEnumConverter<ScriptDocumentEditKind>());

    private const string Script = """
    # Launch
    ## [Intro|Speaker:Alex]
    Keep this.
    Rewrite this line.
    Keep that.
    """;
    private const string DeleteScriptToolName = "script_delete";

    private readonly ScriptAgentToolProvider _provider = new(
        new ScriptDocumentEditService(),
        new ScriptKnowledgeGraphService());

    [Test]
    public void CreateTools_ExposesPredefinedSafeToolSet()
    {
        var tools = _provider.CreateTools(CreateContext());

        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.GetContext);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.ListAppTools);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.RequestAppTool);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.ReadScriptRange);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.ReadEditorSelection);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.ProposeScriptReplacement);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.ProposeScriptInsertion);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.ProposeScriptDeletion);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.ApplyApprovedScriptReplacement);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.ApplyApprovedScriptDeletion);
        Assert.Contains(tools, static tool => tool.Name == ScriptAgentToolNames.BuildScriptGraphSummary);
        Assert.All(tools, static tool => Assert.False(string.IsNullOrWhiteSpace(tool.Description)));
        var applyTool = tools.Single(static tool => tool.Name == ScriptAgentToolNames.ApplyApprovedScriptReplacement);
        Assert.True(applyTool is AIFunction);
        Assert.False(applyTool is ApprovalRequiredAIFunction);
        Assert.True(tools.Single(static tool => tool.Name == ScriptAgentToolNames.ApplyApprovedScriptDeletion) is ApprovalRequiredAIFunction);
    }

    [Test]
    public async Task ReadScriptRange_ReturnsExactRangeFromEditorContext()
    {
        var start = Script.IndexOf("Rewrite", StringComparison.Ordinal);
        var end = start + "Rewrite this line.".Length;
        var readRange = GetFunction(CreateContext(), ScriptAgentToolNames.ReadScriptRange);

        var result = ToResult<ScriptAgentRangeReadResult>(await readRange.InvokeAsync(new AIFunctionArguments(
            new Dictionary<string, object?>
            {
                ["start"] = start,
                ["end"] = end
            })));

        Assert.Equal("Rewrite this line.", result.Text);
        Assert.Equal(new ScriptDocumentRange(start, end), result.Range);
    }

    [Test]
    public async Task ReadEditorSelection_ReturnsCapturedSelection()
    {
        var readSelection = GetFunction(CreateContext(), ScriptAgentToolNames.ReadEditorSelection);

        var result = ToResult<ScriptAgentRangeReadResult>(await readSelection.InvokeAsync());

        Assert.Equal("Rewrite this line.", result.Text);
        Assert.Equal(4, result.Start.Line);
    }

    [Test]
    public async Task ProposeScriptInsertion_ReturnsRevisionBoundInsertPlan()
    {
        var offset = Script.IndexOf("Keep that.", StringComparison.Ordinal);
        var proposeInsertion = GetFunction(CreateContext(), ScriptAgentToolNames.ProposeScriptInsertion);

        var result = ToResult<ScriptAgentEditPreviewResult>(
            await proposeInsertion.InvokeAsync(new AIFunctionArguments(
                new Dictionary<string, object?>
                {
                    ["offset"] = offset,
                    ["insertedText"] = "[pause:500]\n",
                    ["reason"] = "Add a pause before the last beat."
                })));

        Assert.Equal(ScriptDocumentRevision.Create(Script), result.Plan.Revision);
        Assert.Contains(result.Plan.Operations, static operation => operation.Kind == ScriptDocumentEditKind.Insert);
    }

    [Test]
    public async Task ApplyApprovedScriptDeletion_RemovesOnlyApprovedRange()
    {
        var start = Script.IndexOf("Rewrite", StringComparison.Ordinal);
        var end = start + "Rewrite this line.".Length;
        var applyDeletion = GetFunction(CreateContext(), ScriptAgentToolNames.ApplyApprovedScriptDeletion);

        var result = ToResult<ScriptAgentAppliedEditPreviewResult>(
            await applyDeletion.InvokeAsync(new AIFunctionArguments(
                new Dictionary<string, object?>
                {
                    ["start"] = start,
                    ["end"] = end,
                    ["expectedRevision"] = ScriptDocumentRevision.Create(Script).Value,
                    ["reason"] = "User approved deleting this line."
                })));

        Assert.Contains("Keep this.", result.Result.Text, StringComparison.Ordinal);
        Assert.Contains("Keep that.", result.Result.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Rewrite this line.", result.Result.Text, StringComparison.Ordinal);
    }

    [Test]
    public async Task ApplyApprovedScriptReplacement_OnlyMutatesApprovedRange()
    {
        var start = Script.IndexOf("Rewrite", StringComparison.Ordinal);
        var end = start + "Rewrite this line.".Length;
        var applyReplacement = GetFunction(CreateContext(), ScriptAgentToolNames.ApplyApprovedScriptReplacement);

        var result = ToResult<ScriptAgentAppliedEditPreviewResult>(
            await applyReplacement.InvokeAsync(new AIFunctionArguments(
                new Dictionary<string, object?>
                {
                    ["start"] = start,
                    ["end"] = end,
                    ["replacementText"] = "Polish this line.",
                    ["expectedRevision"] = ScriptDocumentRevision.Create(Script).Value,
                    ["reason"] = "User selected this line only."
                })));

        Assert.Contains("Keep this.", result.Result.Text, StringComparison.Ordinal);
        Assert.Contains("Polish this line.", result.Result.Text, StringComparison.Ordinal);
        Assert.Contains("Keep that.", result.Result.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Rewrite this line.", result.Result.Text, StringComparison.Ordinal);
    }

    [Test]
    public async Task BuildScriptGraphSummary_ReturnsCapturedDocumentGraph()
    {
        var graphSummary = GetFunction(CreateContext(), ScriptAgentToolNames.BuildScriptGraphSummary);

        var result = ToResult<ScriptAgentGraphSummaryResult>(await graphSummary.InvokeAsync());

        Assert.True(result.NodeCount > 0);
        Assert.True(result.EdgeCount > 0);
        Assert.Contains(result.FocusLabels, static label => label.Contains("Intro", StringComparison.Ordinal));
    }

    [Test]
    public async Task ListAvailablePrompterOneTools_ReturnsMcpStyleCatalogFromContext()
    {
        var appTool = CreateAppTool(DeleteScriptToolName, requiresApproval: true);
        var listTools = GetFunction(CreateContext([appTool]), ScriptAgentToolNames.ListAppTools);

        var result = ToResult<ScriptAgentAppToolDescriptor[]>(await listTools.InvokeAsync());

        Assert.Single(result);
        Assert.Equal(appTool.Name, result[0].Name);
        Assert.False(result[0].ReadOnly);
        Assert.True(result[0].Destructive);
        Assert.True(result[0].RequiresApproval);
    }

    [Test]
    public async Task RequestPrompterOneTool_ReturnsApprovalStatusForSensitiveTool()
    {
        var requestTool = GetFunction(
            CreateContext([CreateAppTool(DeleteScriptToolName, requiresApproval: true)]),
            ScriptAgentToolNames.RequestAppTool);

        var result = ToResult<ScriptAgentRequestedAppToolResult>(
            await requestTool.InvokeAsync(new AIFunctionArguments(
                new Dictionary<string, object?>
                {
                    ["toolName"] = DeleteScriptToolName,
                    ["argumentsJson"] = "{\"scriptId\":\"draft\"}"
                })));

        Assert.Equal(DeleteScriptToolName, result.ToolName);
        Assert.Equal(ScriptAgentToolStatuses.ApprovalRequired, result.Status);
        Assert.Equal("{\"scriptId\":\"draft\"}", result.ArgumentsJson);
    }

    private static AIFunction GetFunction(ScriptAgentContext context, string name) =>
        (AIFunction)new ScriptAgentToolProvider(new ScriptDocumentEditService(), new ScriptKnowledgeGraphService())
            .CreateTools(context)
            .Single(tool => tool.Name == name);

    private static ScriptAgentContext CreateContext(
        IReadOnlyList<ScriptAgentAppToolDescriptor>? availableTools = null)
    {
        var start = Script.IndexOf("Rewrite", StringComparison.Ordinal);
        var end = start + "Rewrite this line.".Length;

        return new ScriptAgentContext(
            "conversation",
            new ScriptArticleContext(
                Title: "Launch",
                Route: "/editor?id=launch",
                Screen: "Editor",
                Editor: new ScriptEditorContext(
                    DocumentId: "launch",
                    DocumentTitle: "Launch",
                    Content: Script,
                    Revision: ScriptDocumentRevision.Create(Script),
                    Cursor: ScriptDocumentPosition.FromOffset(Script, start),
                    SelectedRange: new ScriptDocumentRange(start, end),
                    SelectedText: "Rewrite this line.",
                    SelectedLineNumbers: [4]),
                AvailableTools: availableTools));
    }

    private static ScriptAgentAppToolDescriptor CreateAppTool(string name, bool requiresApproval) =>
        new(
            name,
            "Delete script",
            "Delete a script from the library.",
            "library",
            "agent",
            null,
            null,
            "delete the script",
            ReadOnly: false,
            Idempotent: false,
            Destructive: true,
            OpenWorld: false,
            RequiresApproval: requiresApproval,
            Parameters: []);

    private static T ToResult<T>(object? result)
    {
        if (result is T typed)
        {
            return typed;
        }

        if (result is JsonElement json)
        {
            return json.Deserialize<T>(JsonOptions) ??
                throw new InvalidOperationException($"Function returned an empty {typeof(T).Name} payload.");
        }

        throw new InvalidOperationException($"Function returned unexpected payload type {result?.GetType().Name ?? "null"}.");
    }
}
