using Microsoft.Extensions.AI;
using PrompterOne.Core.AI.Models;
using PrompterOne.Core.AI.Tools;

namespace PrompterOne.Core.AI.Services;

public sealed class ScriptAgentToolProvider(
    ScriptDocumentEditService documentEditService,
    ScriptKnowledgeGraphService knowledgeGraphService)
{
    public IList<AITool> CreateTools(ScriptAgentContext? context)
    {
        var articleContext = context?.ArticleContext ?? new ScriptArticleContext();
        var contextTools = new ScriptAgentContextTools(articleContext);
        var documentTools = new ScriptAgentDocumentTools(articleContext, documentEditService);
        var graphTools = new ScriptAgentGraphTools(articleContext, knowledgeGraphService);

        return
        [
            ScriptAgentMcpToolFunctionFactory.Create(contextTools.GetActivePrompterContext),
            ScriptAgentMcpToolFunctionFactory.Create(contextTools.ListAvailablePrompterOneTools),
            ScriptAgentMcpToolFunctionFactory.Create(contextTools.RequestPrompterOneTool),
            ScriptAgentMcpToolFunctionFactory.Create(documentTools.FindScriptText),
            ScriptAgentMcpToolFunctionFactory.Create(documentTools.ReadScriptRange),
            ScriptAgentMcpToolFunctionFactory.Create(documentTools.ReadEditorSelection),
            ScriptAgentMcpToolFunctionFactory.Create(documentTools.ProposeScriptReplacement),
            ScriptAgentMcpToolFunctionFactory.Create(documentTools.ProposeScriptInsertion),
            ScriptAgentMcpToolFunctionFactory.Create(documentTools.ProposeScriptDeletion),
            ScriptAgentMcpToolFunctionFactory.Create(documentTools.ApplyApprovedScriptReplacement),
            ScriptAgentMcpToolFunctionFactory.Create(documentTools.ApplyApprovedScriptDeletion),
            ScriptAgentMcpToolFunctionFactory.Create(graphTools.BuildScriptGraphSummaryAsync)
        ];
    }
}
